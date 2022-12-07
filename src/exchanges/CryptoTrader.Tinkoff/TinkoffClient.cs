using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using CodeJam.Collections;
using CryptoTrader.Core;
using CryptoTrader.Core.Helpers;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Utils;
using CryptoTrader.Utils.Throttler;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;
using Context = Tinkoff.Trading.OpenApi.Network.Context;
using Order = CryptoTrader.Core.Models.Orders.Order;
using OrderStatus = Tinkoff.Trading.OpenApi.Models.OrderStatus;
using TinkoffOrder = Tinkoff.Trading.OpenApi.Models.Order;
using static CryptoTrader.Tinkoff.ExchangeRequest;


namespace CryptoTrader.Tinkoff
{
    public class TinkoffClient : IExchangeClient
    {
        private readonly Context context;
        private readonly RequestThrottler throttler;
        private readonly ILogger<TinkoffClient> logger;
        private readonly IMemoryCache cache;
        private readonly IAsyncPolicy policy;

        public TinkoffClient(ApiSecrets secrets, ILogger<TinkoffClient> logger, IMemoryCache cache,
            RequestThrottler requestThrottler)
        {
            var connection = ConnectionFactory.GetConnection(secrets.TinkoffToken);
            context = connection.Context;
            this.logger = logger;
            this.cache = cache;
            policy = BuildPolicy();
            throttler = requestThrottler;
        }

        public async Task<Result<ExchangeData>> GetData(string instrumentId, bool retry = true)
        {
            var result = await Request(async () =>
            {
                var orderBook = await context.MarketOrderbookAsync(instrumentId, 20);
                return orderBook.ToData();
            }, MarketRequest, retry);
            return result;
        }

        public async Task<bool> CancelOrder(string instrumentId, Order order)
        {
            try
            {
                var result = await Request(async () => await context.CancelOrderAsync(order.Id), OrdersCancelRequest);
                if (!result.Success)
                    //hack
                    return result.Error.Message.Contains("ORDER_ERROR: Cannot find order by id");

                var ordersResult = await Request(async () => await context.OrdersAsync(), OrdersRequest);
                if (!ordersResult.Success)
                    return false;
                var target = ordersResult.Value.FirstOrDefault(x => x.OrderId == order.Id);
                if (target == null)
                    return true;
                return target.Status == OrderStatus.Cancelled;
            }
            catch (Exception)
            {
                //это норм, если не смогли заканселить ордер, так как он мог купиться/продаться
                //в следующей итерации синканемся с биржей
                return false;
            }
        }

        public async Task<bool> PlaceOrder(string instrumentId, Order order)
        {
            //TODO обработка ошибок API
            var placeResult = await Request(() => context.PlaceLimitOrderAsync(order.ToOrder(instrumentId)),
                PlaceLimitOrderRequest);
            if (!placeResult.Success)
                return false;
            var placed = placeResult.Value;
            order.Id = placed.OrderId;
            return placed.Status == OrderStatus.New;
        }

        private CacheOrderInfo cacheOrderInfo;

        public async Task<bool> UpdateOrders(string instrumentId, ICollection<Order> localOrders, string tickId)
        {
            Result<List<TinkoffOrder>> ordersResult;
            if (cacheOrderInfo != null && cacheOrderInfo.TickId == tickId)
            {
                ordersResult = cacheOrderInfo.OrderResult;
            }
            else
            {
                ordersResult = await Request(async () => await context.OrdersAsync(),
                    OrdersRequest);
                cacheOrderInfo = new CacheOrderInfo { OrderResult = ordersResult, TickId = tickId };
            }

            if (!ordersResult.Success)
                return false;

            var remoteOrders = ordersResult.Value.Where(x => x.Figi == instrumentId)
                .Select(o => o.ToOrder(instrumentId))
                .ToList();

            foreach (var localOrder in localOrders)
            foreach (var remoteOrder in remoteOrders)
                if (remoteOrder.Id == localOrder.Id)
                {
                    localOrder.Status = remoteOrder.Status;
                    localOrder.OriginalQuantity = remoteOrder.OriginalQuantity;
                    localOrder.ExecutedQuantity = remoteOrder.ExecutedQuantity;
                    localOrder.Price = remoteOrder.Price;
                }

            var unknownOrders = localOrders.ExceptBy(remoteOrders, order => order.Id).ToList();
            var unknownIds = unknownOrders.Select(o => o.Id).ToList();
            var statusResult = await GetStatuses(instrumentId, unknownIds);
            if (!statusResult.Success)
            {
                logger.LogError(statusResult.Error.Message);
                //todo что делаем?
                return true;
            }

            foreach (var unknownOrder in unknownOrders)
            {
                var IdStatus = statusResult.Value.FirstOrDefault(t => t.id == unknownOrder.Id);
                if (IdStatus != default)
                {
                    switch (IdStatus.status)
                    {
                        case Core.Models.Orders.OrderStatus.Canceled:
                            unknownOrder.Status = IdStatus.status;
                            break;
                        case Core.Models.Orders.OrderStatus.Filled:
                            unknownOrder.Status = IdStatus.status;
                            unknownOrder.ExecutedQuantity = unknownOrder.OriginalQuantity;
                            break;
                    }
                }
            }

            return true;
        }

        public async Task<Result<List<(string id, Core.Models.Orders.OrderStatus status)>>> GetStatuses(string instId,
            ICollection<string> ordersIds)
        {
            var historyResult = await Request(
                () => context.OperationsAsync(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow, instId),
                OrdersHistoryRequest);

            if (!historyResult.Success)
                return new Error("Cannot get OrdersHistory");

            return historyResult.Value.Where(op => ordersIds.Contains(op.Id)).Select(op =>
            {
                var status = op.Status switch
                {
                    OperationStatus.Decline => Core.Models.Orders.OrderStatus.Canceled,
                    OperationStatus.Done => Core.Models.Orders.OrderStatus.Filled,
                    OperationStatus.Progress => Core.Models.Orders.OrderStatus.PartiallyFilled,
                    _ => throw new ArgumentOutOfRangeException(nameof(op.Status), "Unknown tinkoff status")
                };
                return (op.Id, status);
            }).ToList();
        }

        [Obsolete]
        public async Task<decimal> GetVolume(string instrumentId)
        {
            var from = DateTime.UtcNow.Date;
            var result = await Request(
                () => context.MarketCandlesAsync(instrumentId, from, DateTime.UtcNow, CandleInterval.QuarterHour),
                MarketRequest);
            return !result.Success ? 0 : result.Value.Candles.Sum(x => x.Volume);
        }

        public async Task<IEnumerable<Kline>> GetKlines(GetCandlesFilter filter, bool useCache = true, bool retry = true)
        {
            if (useCache)
            {
                if (cache.TryGetValue($"GetKlines:{filter.InstrumentId}:{filter.Interval.ToString()}", out List<Kline> s))
                    return s;
            }

            var from = (filter.From ?? DateTimeOffset.UtcNow.Date).DateTime;
            var to = (filter.To ?? DateTimeOffset.UtcNow).DateTime;
            from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var result = await Request(
                () => context.MarketCandlesAsync(filter.InstrumentId, from, to, filter.Interval.ToType()),
                MarketRequest, retry);

            if (!result.Success)
                return null;

            var payload = result.Value.Candles.OrderBy(x => x.Time).AsEnumerable();

            if (filter.Count.HasValue)
                payload = payload.Take(filter.Count.Value).Reverse();

            var candles = payload.Select(x => x.ToCandle())
                .ToList();

            if (useCache)
            {
                cache.Set($"GetKlines:{filter.InstrumentId}:{filter.Interval.ToString()}", candles, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = filter.Interval.ToTime()
                });
            }
            return candles;
        }

        public async Task<IEnumerable<CurrencyPair>> GetExchangeInfo(string baseCurrency)
        {
            if (cache.TryGetValue($"GetExchangeInfo:{baseCurrency}", out List<CurrencyPair> s))
                return s;

            var result = await Request(() => context.MarketStocksAsync(), MarketRequest);
            if (!result.Success)
                return Array.Empty<CurrencyPair>();

            var values = result.Value.Instruments
                .OrderBy(i => i.Name)
                .Where(i => i.Currency == Currency.Usd && i.Type == InstrumentType.Stock)
                .Select(i => new CurrencyPair
                {
                    InstrumentId = i.Figi,
                    Name = i.Name,
                    AmountSignsNumber = 0,
                    PriceSignsNumber = 2
                }).ToList();

            cache.Set($"GetExchangeInfo:{baseCurrency}", values, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
            return values;
        }

        public IDisposable WebSockSubscribe(Func<TradeState, TradeState> act)
        {
            throw new NotImplementedException();
        }

        public ExchangeType ExchangeType { get; } = ExchangeType.Tinkoff;

        private async Task<Result<T>> Request<T>(Func<Task<T>> request, ExchangeRequest requestName, bool retry = true)
        {
            var requestId = Guid.NewGuid().ToString();
            try
            {
                await throttler.Throttle(requestName);
                T result;
                if(retry)
                    result = await policy.ExecuteAsync(request);
                else
                    result = await request();
                return Result.Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error request: {requestName}. {requestId}");
                if (e is OpenApiException apiException)
                    return new Error(apiException.Message);

                return new Error();
            }
        }

        private async Task<Result> Request(Func<Task> request, ExchangeRequest requestName)
        {
            var requestId = Guid.NewGuid().ToString();

            try
            {
                await throttler.Throttle(requestName);
                await policy.ExecuteAsync(request);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error request: {requestName}. {requestId}");
                if (e is OpenApiException apiException)
                    return new Error(apiException.Message);

                return new Error();
            }
            return Result.Ok();
        }

        private AsyncRetryPolicy BuildPolicy()
        {
            return Policy
                .Handle<SocketException>()
                .Or<HttpRequestException>()
                .Or<OperationCanceledException>()
                .Or<OpenApiInvalidResponseException>()
                //если пришла ошибка о том, что ордер на найден, вероятно он закрылся либо отменился
                .Or<OpenApiException>(x => !x.Message.Contains("ORDER_ERROR: Cannot find order by id"))
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(9),
                    TimeSpan.FromSeconds(27),
                    TimeSpan.FromSeconds(81),
                    TimeSpan.FromSeconds(100)
                }, (ex, span, num, ctx) => { logger.LogWarning(ex, $"Retry {num} request"); });
        }
    }
}
