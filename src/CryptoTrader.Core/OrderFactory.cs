using System;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;

namespace CryptoTrader.Core
{
    public static class OrderFactory
    {
        public static Order CreateMarketSellOrder(CurrencyPair pair, decimal quantity)
        {
            var order = CreateOrderInternal(pair, quantity, 1);
            order.Side = OrderSide.Sell;
            order.Type = OrderType.Market;

            return order;
        }

        public static Order CreateMarketBuyOrder(CurrencyPair pair, decimal quantity)
        {
            var order = CreateOrderInternal(pair, quantity, 1);
            order.Side = OrderSide.Sell;
            order.Type = OrderType.Market;

            return order;
        }

        public static Order CreateSellOrder(CurrencyPair pair, decimal quantity, decimal price, decimal partialQuantity)
        {
            var order = CreateOrderInternal(pair, quantity + partialQuantity, price);
            order.Side = OrderSide.Sell;
            order.Type = OrderType.Limit;

            return order;
        }

        public static Order CreateBuyOrder(CurrencyPair pair, decimal quantity, decimal price)
        {
            var order = CreateOrderInternal(pair, quantity, price);
            order.Side = OrderSide.Buy;
            order.Type = OrderType.Limit;
            return order;
        }

        private static Order CreateOrderInternal(CurrencyPair pair, decimal quantity, decimal price)
        {
            var roundAmount = decimal.Round(quantity, pair.AmountSignsNumber, MidpointRounding.AwayFromZero);
            var roundPrice = decimal.Round(price, pair.PriceSignsNumber, MidpointRounding.AwayFromZero);
            return new Order
            {
                TimeInForce = TimeInForce.GoodTillCancel,
                Symbol = pair.InstrumentId,
                OriginalQuantity = roundAmount,
                Price = roundPrice
            };
        }
    }
}
