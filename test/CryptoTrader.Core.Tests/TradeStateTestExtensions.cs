using System.Linq;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;

namespace CryptoTrader.Core.Tests
{
    public static class TradeStateTestExtensions
    {
        public static Order FirstBuyOrder(this TradeState state) =>
            state.ActiveOrders.Where(order => order.Side == OrderSide.Buy)
                .OrderByDescending(order => order.Price)
                .First();

        public static Order SecondBuyOrder(this TradeState state) =>
            state.ActiveOrders.Where(order => order.Side == OrderSide.Buy)
                .OrderByDescending(order => order.Price)
                .Skip(1)
                .First();

        public static void BuyAllOrders(this TradeState state)
        {
            var buyOrder = state.ActiveOrders.Where(order => order.Side == OrderSide.Buy);
            foreach (var order in buyOrder)
            {
                order.Status = OrderStatus.Filled;
                order.ExecutedQuantity = order.OriginalQuantity;
            }
        }

        public static Order FirstNewBuyOrder(this TradeState state) =>
            state.NewOrders.Where(order => order.Side == OrderSide.Buy)
                .OrderByDescending(order => order.Price)
                .First();

        public static Order SellOrder(this TradeState state) =>
            state.ActiveOrders.First(order => order.Side == OrderSide.Sell);

        public static void MakePartial(this Order order, decimal div, int amountSigns)
        {
            order.Status = OrderStatus.PartiallyFilled;
            order.ExecutedQuantity = decimal.Round(order.OriginalQuantity * div, amountSigns);
        }

        public static void MakeFilled(this Order order)
        {
            order.Status = OrderStatus.Filled;
            order.ExecutedQuantity = order.OriginalQuantity;
        }
    }
}
