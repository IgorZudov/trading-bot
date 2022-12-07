using System;
using System.Linq;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using Tinkoff.Trading.OpenApi.Models;
using Order = CryptoTrader.Core.Models.Orders.Order;
using OrderStatus = CryptoTrader.Core.Models.Orders.OrderStatus;
using OrderType = CryptoTrader.Core.Models.Orders.OrderType;
using TinkoffOrder = Tinkoff.Trading.OpenApi.Models;

namespace CryptoTrader.Tinkoff
{
    public static class TinkoffMapper
    {
        public static LimitOrder ToOrder(this Order order, string instrumentId) =>
            new(instrumentId, (int) order.OriginalQuantity, order.Side.ToType(), order.Price);

        public static OperationType ToType(this OrderSide side) => side switch
        {
            OrderSide.Buy => OperationType.Buy,
            OrderSide.Sell => OperationType.Sell
        };

        public static OrderSide ToType(this OperationType side) => side switch
        {
            OperationType.Buy => OrderSide.Buy,
            OperationType.Sell => OrderSide.Sell
        };

        public static OrderType ToType(this TinkoffOrder.OrderType type) => type switch
        {
            TinkoffOrder.OrderType.Limit => OrderType.Limit,
            TinkoffOrder.OrderType.Market => OrderType.Market
        };

        public static Order ToOrder(this TinkoffOrder.Order order, string instrumentId) =>
            new()
            {
                Id = order.OrderId,
                Price = order.Price,
                Side = order.Operation.ToType(),
                OriginalQuantity = order.RequestedLots,
                ExecutedQuantity = order.ExecutedLots,
                TimeInForce = TimeInForce.GoodTillCancel, //todo вспомнить что это
                Type = order.Type.ToType(),
                Symbol = instrumentId,
                Status = order.Status.ToStatus()
            };

        public static OrderStatus ToStatus(this TinkoffOrder.OrderStatus status) => status switch
        {
            TinkoffOrder.OrderStatus.New => OrderStatus.New,
            TinkoffOrder.OrderStatus.PartiallyFill => OrderStatus.PartiallyFilled,
            TinkoffOrder.OrderStatus.Fill => OrderStatus.Filled,
            TinkoffOrder.OrderStatus.Cancelled => OrderStatus.Canceled,
            TinkoffOrder.OrderStatus.PendingCancel => OrderStatus.PendingCancel,
            TinkoffOrder.OrderStatus.Rejected => OrderStatus.Rejected,
            TinkoffOrder.OrderStatus.PendingNew => OrderStatus.New,

            //хз что это, в доке не нашел, ставлю как new
            TinkoffOrder.OrderStatus.Replaced => OrderStatus.New,
            TinkoffOrder.OrderStatus.PendingReplace => OrderStatus.New
        };

        public static CandleInterval ToType(this KlineInterval interval) => interval switch
        {
            KlineInterval.OneMinute => CandleInterval.Minute,
            KlineInterval.FiveMinute => CandleInterval.FiveMinutes,
        };

        public static KlineInterval ToType(this CandleInterval interval) => interval switch
        {
            CandleInterval.Minute => KlineInterval.OneMinute,
            CandleInterval.FiveMinutes => KlineInterval.FiveMinute,
        };

        public static Kline ToCandle(this CandlePayload candle) =>
            new()
            {
                High = candle.High,
                Low = candle.Low,
                Volume = candle.Volume,
                ClosePrice = candle.Close,
                OpenPrice = candle.Open,
                Trades = 100000,
                Time = candle.Time,
                Interval = candle.Interval.ToType()
            };


        public static ExchangeData ToData(this Orderbook book) =>
            new()
            {
                CurrentPrice = book.LastPrice,
                Available = book.TradeStatus == TradeStatus.NormalTrading,
                Asks = book.Asks.Select(x => new OrderbookItem(x.Price, x.Quantity)).ToList(),
                Bids = book.Bids.Select(x => new OrderbookItem(x.Price, x.Quantity)).ToList()
            };
    }
}
