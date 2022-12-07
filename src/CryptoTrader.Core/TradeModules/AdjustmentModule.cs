using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using Microsoft.Extensions.Logging;

namespace CryptoTrader.Core.TradeModules
{
    public class AdjustmentModule : ITradingModule
    {
        private const decimal AdditionalyTakeProfitPercent = 0.1m;
        private readonly TimeSpan reloadingTimeout = TimeSpan.FromMinutes(2);
        private readonly TimeSpan upTakeProfitTime = TimeSpan.FromMinutes(5);
        private readonly decimal defaultFirstStepValue;
        private readonly decimal defaultTakeProfitPercent;
        private readonly CoreConfiguration config;

        private int reloadingCount;
        private DateTime? lastReloadingTime;
        private DateTime? lastSellTime;
        private CurrencyPair lastCurrencyPair;
        private ILogger<AdjustmentModule> logger;
        private decimal? lastTakeProfit;

        public ITradingModule Next { get; set; }

        public AdjustmentModule(CoreConfiguration config, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<AdjustmentModule>();
            defaultFirstStepValue = config.FirstStepDevation;
            defaultTakeProfitPercent = config.TakeProfit;
            this.config = config;
        }

        public async Task ProcessState(TradeState state)
        {
            if (!state.CurrencyPair.Equals(lastCurrencyPair))
                Reset();

            FirstStepAdjsut(state);
            TakeProfitAdjust(state);
            //todo логика перезагрузки шагов ордеров в зависимости от состояния рынка (RISK ON или RISK OFF)

            if (Next != null)
                await Next.ProcessState(state);

            void Reset()
            {
                lastCurrencyPair = state.CurrencyPair;
                lastReloadingTime = null;
                lastSellTime = null;
                reloadingCount = 1;
                lastTakeProfit = null;
                state.FirstStepDeviation = defaultFirstStepValue;
                state.TakeProfit = defaultTakeProfitPercent;
            }
        }

        private void FirstStepAdjsut(TradeState state)
        {
            //если была продажа в последние пять минут, то заходим сразу
            //todo данную статистику нужно хранить отдельно в бд или памяти, чтобы не завязываться на state, сейчас это работает, если сигнал попал в тот же стейт
            if (state.LastSellTime != null && DateTime.UtcNow - state.LastSellTime < TimeSpan.FromMinutes(5))
                state.FirstStepDeviation = 0;
            else
                state.FirstStepDeviation = defaultFirstStepValue;

            if (state.NewOrders.Count == config.OrdersCount
                && state.CancelOrders.Count == config.OrdersCount
                && state.CancelOrders.All(order => order.Side == OrderSide.Buy))
            {
                if (lastReloadingTime == null)
                {
                    reloadingCount++;
                    lastReloadingTime = DateTime.UtcNow;
                    return;
                }

                if (DateTime.UtcNow - lastReloadingTime < reloadingTimeout)
                {
                    reloadingCount++;
                    state.FirstStepDeviation = reloadingCount >= 3 ? 0 : defaultFirstStepValue;
                }
                // todo не покрыто тестами
                else
                {
                    reloadingCount = 1;
                    lastReloadingTime = DateTime.UtcNow;
                    state.FirstStepDeviation = defaultFirstStepValue;
                }
            }
        }

        private void TakeProfitAdjust(TradeState state)
        {
            if (state.ActiveOrders.SellOrders().WithStatus(OrderStatus.Filled).Any())
            {
                if (lastSellTime == null)
                {
                    lastSellTime = DateTime.UtcNow;
                    return;
                }

                if (DateTime.UtcNow - lastSellTime <= upTakeProfitTime)
                {
                    var takeProfit = (lastTakeProfit ?? defaultTakeProfitPercent) + AdditionalyTakeProfitPercent;
                    lastTakeProfit = takeProfit;
                    state.TakeProfit = takeProfit;
                }
                else
                {
                    state.TakeProfit = defaultTakeProfitPercent;
                    lastTakeProfit = null;
                }
            }
        }
    }
}
