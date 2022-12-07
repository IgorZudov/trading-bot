using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;

namespace CryptoTrader.Core
{
    public static class TradeStateOrdersExt
    {
        public static bool IsNotInDeal(this IEnumerable<Order> orders) =>
            orders.All(order => order.Status == OrderStatus.New);

        public static bool IsInDeal(this IEnumerable<Order> orders) =>
            orders.Any(order => order.Status != OrderStatus.New);

        public static bool IsInDeal(this TradeState state) =>
            state.BuyedOrders.Any();

        public static bool CanChangeInstrument(this TradeState state) =>
            state.ActiveOrders.Count == 0 && state.NewOrders.Count == 0;

        public static IEnumerable<Order> BuyOrders(this IEnumerable<Order> orders) =>
            orders.Where(order => order.Side == OrderSide.Buy);

        public static IEnumerable<Order> SellOrders(this IEnumerable<Order> orders) =>
            orders.Where(order => order.Side == OrderSide.Sell);

        public static IEnumerable<Order> WithStatus(this IEnumerable<Order> orders, OrderStatus status) =>
            orders.Where(order => order.Status == status);
    }
}
