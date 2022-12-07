using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using JetBrains.Annotations;

namespace CryptoTrader.Core.Helpers
{
    public static class TradeStateDsl
    {
        [CanBeNull]
        public static TradeState IfActiveSellOrdersNotExist(this TradeState state, Action action)
        {
            return ExecuteStep(state, tradeState =>
            {
                if (!state.ActiveOrders.SellOrders().Any()
                    && state.BuyOrdersPrice < state.ExchangeData.CurrentPrice
                    && !state.NewOrders.SellOrders().Any())
                {
                    action.Invoke();
                    return null;
                }
                return state;
            });
        }

        [CanBeNull]
        public static TradeState IfBuyOrdersBoughtWhenSellOrderNotExist(this TradeState state,
            Action<ICollection<Order>> action)
        {
            return ExecuteStep(state, tradeState =>
            {
                if (!state.ActiveOrders.SellOrders().Any()
                    && state.ActiveOrders.Any(_ => _.Status == OrderStatus.Filled))
                {
                    state.LastFirstBuyTime = DateTime.UtcNow;
                    action.Invoke(state.ActiveOrders.WithStatus(OrderStatus.Filled).ToList());
                    return null;
                }
                return state;
            });
        }

        [CanBeNull]
        public static TradeState IfBuyOrdersBoughtWhenSellOrderExist(this TradeState state,
            Action<Order, ICollection<Order>> action)
        {
            return ExecuteStep(state, tradeState =>
            {
                var sellOrder = state.ActiveOrders.FirstOrDefault(_ => _.Side == OrderSide.Sell);
                var filledBuyOrders = state.ActiveOrders.BuyOrders().WithStatus(OrderStatus.Filled).ToList();
                if (filledBuyOrders.Any())
                {
                    if (sellOrder == null)
                        return null;

                    action.Invoke(sellOrder, filledBuyOrders);
                    return null;
                }
                return state;
            });
        }

        [CanBeNull]
        public static TradeState IfSellOrderComplited(this TradeState state, Action<Order> action)
        {
            return ExecuteStep(state, tradeState =>
            {
                var complitedSellOrder = state.ActiveOrders
                    .FirstOrDefault(_ => _.Side == OrderSide.Sell && _.Status == OrderStatus.Filled);
                if (complitedSellOrder != null)
                {
                    state.LastSellTime = DateTime.UtcNow;
                    action.Invoke(complitedSellOrder);
                }
                return state;
            });
        }

        [CanBeNull]
        public static TradeState IfHasNewSellOrder(this TradeState state, Action<Order> action)
        {
            return ExecuteStep(state, tradeState =>
            {
                var sell = state.NewOrders.SellOrders().SingleOrDefault();
                if (sell != null)
                {
                    action.Invoke(sell);
                }
                return state;
            });
        }

        private static TradeState? ExecuteStep([CanBeNull] TradeState state, Func<TradeState, TradeState?> action)
        {
            if (state != null)
            {
                return action(state);
            }
            return null;
        }
    }
}
