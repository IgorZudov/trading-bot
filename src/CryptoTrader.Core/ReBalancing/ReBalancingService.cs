using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Buy;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.MarketWorkMode;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Rebalancing
{
    public class ReBalancingService
    {
        private readonly CoreConfiguration configuration;
        private readonly ReBalancingOptions options;
        private readonly MarketWorkModeProvider modeProvider;
        private readonly BalanceCalculator depthCalculator;

        public ReBalancingService(CoreConfiguration configuration,
            ReBalancingOptions options,
            MarketWorkModeProvider modeProvider,
            BalanceCalculator depthCalculator)
        {
            this.configuration = configuration;
            this.options = options;
            this.modeProvider = modeProvider;
            this.depthCalculator = depthCalculator;
        }

        public void ReBalance(List<TradeState> states)
        {
            var readyStates = states
                .Where(x => x.CurrencyPair != null)
                .OrderByDescending(x => x.SpentDeposit)
                .ToList();

            var inDealStates = readyStates.Where(x => x.IsInDeal()).ToList();
            var freeStates = readyStates
                .Except(inDealStates)
                .DistinctBy(x => x.CurrencyPair.InstrumentId).ToList();

            var maxDepth = GetMaxDepth();

            var availableDeposit = configuration.LimitDeposit;
            var availableDepth = maxDepth;
            var positionMargin = options.GetPositionMargin(modeProvider.CurrentState);

            BalanceInDealStates();
            BalanceFreeStates();

            foreach (var state in states)
                state.CalculateMaxBuyDepth();

            void BalanceInDealStates()
            {
                foreach (var inDealState in inDealStates)
                {
                    var executedCount = inDealState.BuyedOrders.Count;
                    var buyCount = inDealState.ActiveOrders.BuyOrders().Count();
                    var needCount = positionMargin - buyCount;

                    availableDepth -= executedCount;

                    if (availableDepth - needCount < 0)
                    {
                        var extraDepth = Math.Abs(availableDepth - needCount);
                        var receivedDepth = 0;

                        while (receivedDepth < extraDepth)
                        {
                            var extraState = freeStates.FirstOrDefault(x => x.IsActive);
                            if (extraState == null)
                                break;

                            var cancelled = extraState.ActiveOrders.BuyOrders().Count();
                            receivedDepth += cancelled;
                            extraState.CancelActiveOrders();
                            extraState.IsActive = false;
                        }

                        availableDepth += receivedDepth;
                    }

                    if (availableDepth - needCount < 0)
                        needCount = availableDepth;

                    var totalDepth = executedCount + buyCount + needCount;
                    var limitDeposit = depthCalculator.CalculateLimitDeposit(inDealState, totalDepth);
                    if (availableDeposit - limitDeposit >= 0)
                    {
                        inDealState.LimitDeposit = limitDeposit;
                        availableDeposit -= limitDeposit;
                    }
                    else
                    {
                        if(availableDeposit >= 0)
                            inDealState.LimitDeposit = availableDeposit;
                        availableDeposit = 0;
                    }
                }
            }

            void BalanceFreeStates()
            {
                foreach (var freeState in freeStates)
                {
                    if (availableDepth - positionMargin >= 0)
                    {
                        availableDepth -= positionMargin;
                        var limitDeposit = depthCalculator.CalculateLimitDeposit(freeState, positionMargin);
                        if (availableDeposit - limitDeposit >= 0)
                        {
                            freeState.LimitDeposit = limitDeposit;
                            availableDeposit -= limitDeposit;
                            freeState.IsActive = true;
                        }
                        else
                        {
                            //todo реализовать метод деактивации стейта
                            freeState.CancelActiveOrders();
                            freeState.IsActive = false;
                        }
                    }
                    else
                    {
                        freeState.CancelActiveOrders();
                        freeState.IsActive = false;
                    }
                }
            }

            int GetMaxDepth()
            {
                //получаем минимальную глубину инструмента
                //ее будем считать максимально допустимой для системы
                var minDepth = int.MaxValue;

                foreach (var readyState in readyStates)
                {
                    readyState.LimitDeposit = configuration.LimitDeposit;
                    var depth = depthCalculator.CalculateBuyDepth(readyState);
                    if (depth < minDepth)
                        minDepth = depth;

                    //сбрасываем лимит
                    readyState.LimitDeposit = 0;
                }

                if (minDepth == int.MaxValue)
                    return 0;

                return minDepth;
            }
        }
    }
}
