using System;
using Binance.Net.Objects;
using CryptoTrader.Core.Models.Orders;
using BinanceOrderSide = Binance.Net.Objects.OrderSide;
using OrderSide = CryptoTrader.Core.Models.Orders.OrderSide;
using BinanceOrderType = Binance.Net.Objects.OrderType;
using OrderType = CryptoTrader.Core.Models.Orders.OrderType;
using BinanceTimeInForce = Binance.Net.Objects.TimeInForce;
using TimeInForce = CryptoTrader.Core.Models.TimeInForce;
using BinanceOrderStatus = Binance.Net.Objects.OrderStatus;
using OrderStatus = CryptoTrader.Core.Models.Orders.OrderStatus;

namespace CryptoTrader.Binance
{
    public static class BinanceOrderMapper
    {
        public static BinanceOrder MapToBinanceOrder(Order order) => new()
        {
            Side = ToBinanceOrderSide(order.Side),
            Type = ToBinanceOrderType(order.Type),
            OriginalQuantity = order.OriginalQuantity,
            Price = order.Price,
            TimeInForce = ToBinanceTimeInForce(order.TimeInForce),
            Status = ToBinanceOrderStatus(order.Status)
        };

        public static Order MapFromBinance(BinanceOrder order) => new()
        {
            Side = ToOrderSide(order.Side),
            Type = ToOrderType(order.Type),
            OriginalQuantity = order.OriginalQuantity,
            Price = order.Price,
            TimeInForce = ToTimeInForce(order.TimeInForce),
            Status = ToOrderStatus(order.Status),
            ExecutedQuantity = order.ExecutedQuantity
        };

        public static BinanceOrderSide ToBinanceOrderSide(OrderSide side)
        {
            switch (side)
            {
                case OrderSide.Buy:
                    return BinanceOrderSide.Buy;
                case OrderSide.Sell:
                    return BinanceOrderSide.Sell;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public static OrderSide ToOrderSide(BinanceOrderSide side)
        {
            switch (side)
            {
                case BinanceOrderSide.Buy:
                    return OrderSide.Buy;
                case BinanceOrderSide.Sell:
                    return OrderSide.Sell;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public static OrderStatus ToOrderStatus(BinanceOrderStatus status)
        {
            switch (status)
            {
                case BinanceOrderStatus.New:
                    return OrderStatus.New;
                case BinanceOrderStatus.PartiallyFilled:
                    return OrderStatus.PartiallyFilled;
                case BinanceOrderStatus.Filled:
                    return OrderStatus.Filled;
                case BinanceOrderStatus.Canceled:
                    return OrderStatus.Canceled;
                case BinanceOrderStatus.PendingCancel:
                    return OrderStatus.PendingCancel;
                case BinanceOrderStatus.Rejected:
                    return OrderStatus.Rejected;
                case BinanceOrderStatus.Expired:
                    return OrderStatus.Expired;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static BinanceOrderStatus ToBinanceOrderStatus(OrderStatus orderStatus)
        {
            switch (orderStatus)
            {
                case OrderStatus.New:
                    return BinanceOrderStatus.New;
                case OrderStatus.PartiallyFilled:
                    return BinanceOrderStatus.PartiallyFilled;
                case OrderStatus.Filled:
                    return BinanceOrderStatus.Filled;
                case OrderStatus.Canceled:
                    return BinanceOrderStatus.Canceled;
                case OrderStatus.PendingCancel:
                    return BinanceOrderStatus.PendingCancel;
                case OrderStatus.Rejected:
                    return BinanceOrderStatus.Rejected;
                case OrderStatus.Expired:
                    return BinanceOrderStatus.Expired;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, null);
            }
        }

        public static TimeInForce ToTimeInForce(BinanceTimeInForce side)
        {
            switch (side)
            {
                case BinanceTimeInForce.GoodTillCancel:
                    return TimeInForce.GoodTillCancel;
                case BinanceTimeInForce.ImmediateOrCancel:
                    return TimeInForce.ImmediateOrCancel;
                /* case BinanceTimeInForce.FillOrKill:
                     return TimeInForce.FillOrKill;*/
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public static BinanceTimeInForce ToBinanceTimeInForce(TimeInForce side)
        {
            switch (side)
            {
                case TimeInForce.GoodTillCancel:
                    return BinanceTimeInForce.GoodTillCancel;
                case TimeInForce.ImmediateOrCancel:
                    return BinanceTimeInForce.ImmediateOrCancel;
                /* case TimeInForce.FillOrKill:
                     return BinanceTimeInForce.;*/
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }


        public static OrderType ToOrderType(BinanceOrderType side)
        {
            switch (side)
            {
                case BinanceOrderType.Market:
                    return OrderType.Market;
                case BinanceOrderType.Limit:
                    return OrderType.Limit;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public static BinanceOrderType ToBinanceOrderType(OrderType side)
        {
            switch (side)
            {
                case OrderType.Market:
                    return BinanceOrderType.Market;
                case OrderType.Limit:
                    return BinanceOrderType.Limit;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
    }
}
