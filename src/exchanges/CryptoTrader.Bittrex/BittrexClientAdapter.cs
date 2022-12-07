using System;
using System.Collections.Generic;
using Bittrex.Net;
using Bittrex.Net.Objects;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Polly;
using Order = CryptoTrader.Core.Models.Orders.Order;
using OrderType = Bittrex.Net.Objects.OrderType;

namespace CryptoTrader.Bittrex
{
    public class BittrexClientAdapter //: IExchangeClient
    {
        //todo add to secrets.json
        private const string ApiKey = "*";
        private const string ApiSecret = "*";
        private readonly TimeSpan delay = TimeSpan.FromSeconds(1);
        private readonly ILogger<BittrexClientAdapter> logger;

        public BittrexClientAdapter(ILoggerFactory loggerFactory)
        {
            BittrexDefaults.SetDefaultApiCredentials(ApiKey, ApiSecret);
            logger = loggerFactory.CreateLogger<BittrexClientAdapter>();
        }

        public decimal? GetCurrentPrice(string currency)
        {
            var policy = BuildPolicy<decimal?>(null);
            var result = policy.Execute(() =>
            {
                using (var client = new BittrexClient())
                {
                    var ticker = client.GetTickerAsync(currency).Result;
                    if (!ticker.Success)
                    {
                        logger.LogWarning($"Failed call API {ticker.Error.ErrorMessage}");
                        throw new InvalidOperationException();
                    }

                    return ticker.Result.Last;
                }
            });
            return result;
        }

        [CanBeNull]
        public ExchangeData GetData(string instrumentId)
        {
            var currentPrice = GetCurrentPrice(instrumentId);
            if (!currentPrice.HasValue)
            {
                return null;
            }

            return new ExchangeData
            {
                CurrentPrice = currentPrice.Value
            };
        }

        public bool CancelOrder(string currency, Order order)
        {
            logger.LogTrace($"Cancel order in {currency} :" + order);
            var policy = BuildPolicy(false);
            var result = policy.Execute(() =>
            {
                using (var client = new BittrexClient())
                {
                    var resultApi = client.CancelOrder(Guid.Parse(order.Id));
                    if (!resultApi.Success)
                    {
                        logger.LogWarning($"Failed call API {resultApi.Error.ErrorMessage}");
                        throw new InvalidOperationException();
                    }

                    return resultApi.Success;
                }
            });
            return result;
        }

        public bool PlaceOrder(string currency, Order order)
        {
            logger.LogTrace($"Place order in {currency} :" + order);
            var policy = BuildPolicy(false);
            var result = policy.Execute(() =>
            {
                using (var client = new BittrexClient())
                {
                    BittrexApiResult<BittrexGuid> resultApi;
                    if (order.Side == OrderSide.Buy)
                    {
                        resultApi = client.PlaceOrder(OrderType.Buy, currency, order.OriginalQuantity,
                            order.Price);
                    }
                    else
                    {
                        resultApi = client.PlaceOrder(OrderType.Sell, currency, order.OriginalQuantity,
                            order.Price);
                    }

                    if (!resultApi.Success)
                    {
                        logger.LogWarning($"Failed call API {resultApi.Error.ErrorMessage}");
                        throw new InvalidOperationException();
                    }

                    order.Id = resultApi.Result.Uuid.ToString();
                    return resultApi.Success;
                }
            });
            return result;
        }

        public bool UpdateOrders(string currency, ICollection<Order> orders)
        {
            var policy = BuildPolicy(false);
            var result = policy.Execute(() =>
            {
                bool success = true;
                using (var client = new BittrexClient())
                {
                    foreach (var order in orders)
                    {
                        var resultApi = client.GetOrder(Guid.Parse(order.Id));
                        if (!resultApi.Success)
                        {
                            logger.LogWarning($"Failed call API {resultApi.Error.ErrorMessage}");
                            throw new InvalidOperationException();
                        }

                        success &= resultApi.Success;
                        var openOrder = resultApi.Result;
                        order.Status = openOrder.Quantity == openOrder.QuantityRemaining
                            ? OrderStatus.New
                            : openOrder.QuantityRemaining > 0
                                ? OrderStatus.PartiallyFilled
                                : OrderStatus.Filled;
                        order.Symbol = currency;
                        order.OriginalQuantity = openOrder.Quantity;
                        order.ExecutedQuantity = openOrder.Quantity - openOrder.QuantityRemaining;
                    }
                }

                return success;
            });
            return result;
        }

        public decimal GetVolume(string currency)
        {
            throw new NotImplementedException();
        }

        public ICollection<Kline> GetKlines(string currency, int count, KlineInterval interval)
        {
            throw new NotImplementedException();
        }

        public ICollection<CurrencyPair> GetExchangeInfo(string coin)
        {
            throw new NotImplementedException();
        }

        public IDisposable WebSockSubscribe(Func<TradeState, TradeState> act)
        {
            throw new NotImplementedException();
        }

        private Policy<T> BuildPolicy<T>([CanBeNull] T fallbackResult)
        {
            var fallback = Policy<T>.Handle<InvalidOperationException>()
                .Fallback(token => fallbackResult, onFallback: result => logger.LogError($"Api not enable"));
            var retry = Policy
                .Handle<InvalidOperationException>()
                .WaitAndRetry(3, i => delay);
            return fallback.Wrap(retry);
        }
    }
}
