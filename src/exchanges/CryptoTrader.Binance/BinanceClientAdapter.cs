using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using CryptoTrader.Utils;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Polly;
using OrderStatus = CryptoTrader.Core.Models.Orders.OrderStatus;

namespace CryptoTrader.Binance
{
    public class BinanceClientAdapter  : IExchangeClient
    {
        private readonly TimeSpan requestTimeout = TimeSpan.FromMilliseconds(700);
        private readonly ILogger<BinanceClientAdapter> logger;

        //todo add to secrets.json
        private BinanceClientOptions Options => new()
        {
            ApiCredentials = new ApiCredentials("*",
                "*")
        };

        public BinanceClientAdapter(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<BinanceClientAdapter>();
        }

        public async Task<decimal?> GetCurrentPrice(string currency)
        {
            var result = ExecuteSave<decimal?>(() =>
            {
                using (var client = new BinanceClient(Options))
                {
                    var resultApi = client.Get24HPrice(currency);
                    if (!resultApi.Success)
                    {
                        throw new InvalidOperationException(
                            $"Failed API call {nameof(GetCurrentPrice)} " +
                            $"with args {nameof(currency)} : {currency}. " +
                            $"Message : {resultApi.Error.Message}");
                    }

                    return resultApi.Data.LastPrice;
                }
            }, null);
            await Task.Delay(requestTimeout);
            return result;
        }

        public async Task<Result<ExchangeData>> GetData(string instrumentId, bool retry = true)
        {
            var currentPrice = await GetCurrentPrice(instrumentId);
            if (!currentPrice.HasValue)
                return new Error();

            return new ExchangeData
            {
                CurrentPrice = currentPrice.Value
            };
        }

        public async Task<bool> CancelOrder(string instrumentId, Order order)
        {
            logger.LogInformation("Cancel order :" + order);
            var result = ExecuteSave(() =>
            {
                using var client = new BinanceClient(Options);
                var resultApi = client.CancelOrder(instrumentId, int.Parse(order.Id));
                if (!resultApi.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed API call {nameof(CancelOrder)}. " +
                        $"with args {nameof(instrumentId)} : {instrumentId}. " +
                        $"with args {nameof(order)} : {order}. " +
                        $"Message : {resultApi.Error.Message}");
                }

                return resultApi.Success;
            }, false);
            await Task.Delay(requestTimeout);
            return result;
        }

        public async Task<bool> PlaceOrder(string instrumentId, Order order)
        {
            logger.LogInformation("Place order :" + order);
            var result = ExecuteSave(() =>
            {
                var bo = BinanceOrderMapper.MapToBinanceOrder(order);
                using var client = new BinanceClient(Options);
                var resultApi = client.PlaceOrder(instrumentId, bo.Side, bo.Type,
                    bo.OriginalQuantity,
                    null,
                    null,
                    bo.Price,
                    bo.TimeInForce);
                if (!resultApi.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed API call {nameof(PlaceOrder)}. " +
                        $"with args {nameof(instrumentId)} : {instrumentId}. " +
                        $"with args {nameof(order)} : {order}. " +
                        $"Message : {resultApi.Error.Message}");
                }

                order.Id = resultApi.Data.OrderId.ToString();
                return resultApi.Success;
            }, false);
            await Task.Delay(requestTimeout);
            return result;
        }

        public async Task<bool> UpdateOrders(string instrumentId, ICollection<Order> availableOrders, string tickId)
        {
            var result = ExecuteSave(() =>
            {
                using var client = new BinanceClient(Options);
                var resultApi = client.GetAllOrders(instrumentId);
                if (!resultApi.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed API call {nameof(UpdateOrders)}. Message : {resultApi.Error.Message}");
                }

                foreach (var order in availableOrders)
                foreach (var binOrder in resultApi.Data)
                    if (binOrder.OrderId.ToString() == order.Id)
                    {
                        var updated = BinanceOrderMapper.MapFromBinance(binOrder);
                        order.Status = updated.Status;
                        order.OriginalQuantity = updated.OriginalQuantity;
                        order.ExecutedQuantity = updated.ExecutedQuantity;
                    }

                return resultApi.Success;
            }, false);
            await Task.Delay(requestTimeout);
            return result;
        }

        public Task<Result<List<(string id, OrderStatus status)>>> GetStatuses(string instId,
            ICollection<string> ordersIds)
        {
            throw new NotImplementedException();
        }

        public async Task<decimal> GetVolume(string instrumentId)
        {
            var result = ExecuteSave(() =>
            {
                using var client = new BinanceClient(Options);
                var resultApi = client.Get24HPrice(instrumentId);
                if (!resultApi.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed API call {nameof(GetVolume)}. " +
                        $"with args {nameof(instrumentId)} : {instrumentId}. " +
                        $"Message : {resultApi.Error.Message}");
                }

                return resultApi.Data.Volume * resultApi.Data.BidPrice;
            }, 0);
            await Task.Delay(requestTimeout);
            return result;
        }


        Task<IEnumerable<CurrencyPair>> IExchangeClient.GetExchangeInfo(string baseCurrency) => throw new NotImplementedException();

        public async Task<IEnumerable<Kline>> GetKlines(GetCandlesFilter filter, bool useCache = true, bool retry = true)
        {
            var result = ExecuteSave(() =>
            {
                using var client = new BinanceClient(Options);
                var resultApi = client
                    .GetKlines(filter.InstrumentId, BinanceKlineMapper.ToKlineInterval(filter.Interval), null, null, filter.Count);
                if (!resultApi.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed API call {nameof(GetKlines)}. " +
                        $"with args {nameof(filter.InstrumentId)} : {filter.InstrumentId}. " +
                        $"with args {nameof(filter.Count)} : {filter.Count}. " +
                        $"with args {nameof(filter.Interval)} : {filter.Interval}. " +
                        $"Message : {resultApi.Error.Message}");
                }

                Task.Delay(requestTimeout).Wait();
                return resultApi.Data.Select(BinanceKlineMapper.MapFromBinance).ToList();
            }, new List<Kline>());
            await Task.Delay(requestTimeout);
            return result;
        }

        public async Task<ICollection<CurrencyPair>> GetExchangeInfo(string baseCurrency)
        {
            var result = ExecuteSave(() =>
            {
                var currencyPairs = new List<CurrencyPair>();
                using var client = new BinanceClient(Options);
                var resultApi = client.GetExchangeInfo();
                if (!resultApi.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed API call {nameof(GetExchangeInfo)}. " +
                        $"with args {nameof(baseCurrency)} : {baseCurrency}. " +
                        $"Message : {resultApi.Error.Message}");
                }

                var symbols = resultApi.Data.Symbols.Where(_ => _.Status == SymbolStatus.Trading);
                var pairs = symbols.Where(_ => _.Name.EndsWith(baseCurrency));
                currencyPairs.AddRange(pairs.Select(BinanceSymbolMapper.MapFromBinance));
                return currencyPairs;
            }, new List<CurrencyPair>());
            await Task.Delay(requestTimeout);
            return result;
        }

        public IDisposable WebSockSubscribe(Func<TradeState, TradeState> act)
        {
            throw new NotImplementedException();
        }

        public ExchangeType ExchangeType { get; } = ExchangeType.Binance;

        public async Task<decimal> GetBalance(string currency)
        {
            using var client = new BinanceClient(Options);
            var accountInfo = await client.GetAccountInfoAsync();
            return accountInfo.Data.Balances.First(balance => balance.Asset == currency).Free;
        }

        private T ExecuteSave<T>(Func<T> action, [CanBeNull] T fallbackResult)
        {
            var fallback = Policy<T>.Handle<InvalidOperationException>()
                .Fallback(token => fallbackResult, result => logger.LogError($"Api not enable"));
            var retry = Policy
                .Handle<InvalidOperationException>()
                .WaitAndRetry(3, i => requestTimeout, (exception, span) => logger.LogWarning(exception.Message));
            return fallback.Wrap(retry).Execute(action);
        }
    }
}
