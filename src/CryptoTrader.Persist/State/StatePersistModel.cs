using System;
using System.Collections.Generic;
using CodeJam.Strings;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.HypeAnalyzer;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using LiteDB;

namespace CryptoTrader.Persist.State
{
    internal class StatePersistModel
    {
        [BsonId]
        public object Id { get; set; }

        public decimal PartialCoinsAmount { get; set; }

        public CurrencyPair CurrencyPair { get; set; }

        /// <summary>
        ///         Активные ордера
        /// </summary>
        public List<Order> ActiveOrders { get; set; } = new();

        /// <summary>
        /// Цены, по которым купились ордера
        /// </summary>
        public List<Order> BuyedOrders { get; set; } = new();

        /// <summary>
        ///         Ордера созданные на текущем прогоне
        /// </summary>
        public List<Order> NewOrders { get; set; } = new();

        /// <summary>
        ///         Ордера которые будут отменены на текущем прогоне
        /// </summary>
        public List<Order> CancelOrders { get; set; } = new();

        public int MaxBuyDepth { get; set; }

        public decimal BuyOrdersPrice { get; set; }


        public decimal LimitDeposit { get; set; }

        public int MaxOrderCount { get; set; }

        public decimal TakeProfit { get; set; }

        public decimal FirstStepDeviation { get; set; }

        public bool CanBalance { get; set; }

        public decimal CalculatedDepositOrder { get; set; }
        /// <summary>
        /// Время выставления первых покупок последней сделки
        /// </summary>
        public DateTime? LastDealSetTime { get; set; }

        /// <summary>
        /// Время совершения первой покупки последней сделки
        /// </summary>
        public DateTime? LastFirstBuyTime { get; set; }

        /// <summary>
        /// Время совершения продажи последнй сделки
        /// </summary>
        public DateTime? LastSellTime { get; set; }

        /// <summary>
        ///         Последние данные бирже, полученные стейтом
        /// </summary>
        public ExchangeData? ExchangeData { get; set; }

        /// <summary>
        /// Инофрмация по текущему сигналу
        /// </summary>
        public SignalInfo? SignalInfo { get; set; }


        public static StatePersistModel FromState(TradeState state)
        {
            return new StatePersistModel
            {
                PartialCoinsAmount = state.PartialCoinsAmount,
                CurrencyPair = state.CurrencyPair,
                ActiveOrders = state.ActiveOrders,
                BuyedOrders = state.BuyedOrders,
                NewOrders = state.NewOrders,
                CancelOrders = state.CancelOrders,
                MaxBuyDepth = state.MaxBuyDepth,
                BuyOrdersPrice = state.BuyOrdersPrice,
                CalculatedDepositOrder = state.CalculatedDepositOrder,
                LimitDeposit = state.LimitDeposit,
                MaxOrderCount = state.MaxOrderCount,
                TakeProfit = state.TakeProfit,
                FirstStepDeviation = state.FirstStepDeviation,
                Id = state.Id.IsNullOrEmpty() ? null : new ObjectId(state.Id),
                LastSellTime = state.LastSellTime,
                LastFirstBuyTime = state.LastFirstBuyTime,
                LastDealSetTime = state.LastDealSetTime,
                ExchangeData = state.ExchangeData,
                SignalInfo = state.SignalInfo
            };
        }

        public static TradeState ToState(StatePersistModel model, CoreConfiguration configuration)
        {
            var state = new TradeState(configuration)
            {
                PartialCoinsAmount = model.PartialCoinsAmount,
                CurrencyPair = model.CurrencyPair,
                MaxBuyDepth = model.MaxBuyDepth,
                BuyOrdersPrice = model.BuyOrdersPrice,
                Id = model.Id.ToString()!,
                CalculatedDepositOrder = model.CalculatedDepositOrder,
                LimitDeposit = model.LimitDeposit,
                MaxOrderCount = model.MaxOrderCount,
                TakeProfit = model.TakeProfit,
                FirstStepDeviation = model.FirstStepDeviation,
                LastSellTime = model.LastSellTime,
                LastFirstBuyTime = model.LastFirstBuyTime,
                LastDealSetTime = model.LastDealSetTime,
                ExchangeData = model.ExchangeData,
                SignalInfo = model.SignalInfo
            };
            state.NewOrders.AddRange(model.NewOrders);
            state.ActiveOrders.AddRange(model.ActiveOrders);
            state.BuyedOrders.AddRange(model.BuyedOrders);
            state.CancelOrders.AddRange(model.CancelOrders);
            return state;
        }
    }
}
