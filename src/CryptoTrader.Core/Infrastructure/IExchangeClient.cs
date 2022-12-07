using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using CryptoTrader.Utils;
using JetBrains.Annotations;

namespace CryptoTrader.Core.Infrastructure
{
    public interface IExchangeClient
    {
        [ItemCanBeNull]
        Task<Result<ExchangeData>> GetData(string instrumentId, bool retry = true);

        Task<bool> CancelOrder(string instrumentId, Order order);

        Task<bool> PlaceOrder(string instrumentId, Order order);

        Task<bool> UpdateOrders(string instrumentId, ICollection<Order> availableOrders, string tickId);

        public Task<Result<List<(string id, OrderStatus status)>>> GetStatuses(string instId,
            ICollection<string> ordersIds);

        Task<decimal> GetVolume(string instrumentId);

        Task<IEnumerable<Kline>> GetKlines(GetCandlesFilter filter, bool useCache = true, bool retry = true);

        Task<IEnumerable<CurrencyPair>> GetExchangeInfo(string baseCurrency);

        IDisposable WebSockSubscribe(Func<TradeState, TradeState> act);

        ExchangeType ExchangeType { get; }
    }
}
