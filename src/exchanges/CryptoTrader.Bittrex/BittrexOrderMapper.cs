using System;
using Bittrex.Net.Objects;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using BittrexOrderSide = Bittrex.Net.Objects.OrderType;

namespace CryptoTrader.Bittrex
{
    public static class BittrexOrderMapper
    {

        public static Order MapFromBittrex(BittrexAccountOrder order) => new Order
        {
            Side = order.Type == OrderTypeExtended.LimitBuy ? OrderSide.Buy : OrderSide.Sell,
            Status = order.IsOpen ? OrderStatus.New : OrderStatus.Filled,
            OriginalQuantity = order.Quantity,
            Price = order.Price,
            TimeInForce = TimeInForce.GoodTillCancel
        };

        public static BittrexOrderSide ToBittrexOrderSide(OrderSide side)
        {
            switch (side)
            {
                case OrderSide.Buy:
                    return BittrexOrderSide.Buy;
                case OrderSide.Sell:
                    return BittrexOrderSide.Sell;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
    }
}
