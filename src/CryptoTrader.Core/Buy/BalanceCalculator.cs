using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Helpers;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Buy
{
    public class BalanceCalculator
    {
        private readonly CoreConfiguration config;

        public BalanceCalculator(CoreConfiguration config)
        {
            this.config = config;
        }

        public int CalculateBuyDepth(TradeState state)
        {
            if (!state.CanBalance)
                return 0;

            var resultDepth = 0;
            var (defaultQuantity, defaultPrice, limit) = GetParameters(state);

            while (limit < state.LimitDeposit)
            {
                if (limit <= state.LimitDeposit)
                    resultDepth++;

                defaultQuantity *= ((100m + config.MartinPercent) / 100).Round(state.CurrencyPair.AmountSignsNumber);
                //todo менять defaultPrice c plusStep
                var lastStep = defaultQuantity * defaultPrice;
                limit += lastStep;
            }
            return resultDepth;
        }


        public decimal CalculateLimitDeposit(TradeState state, int needDepth)
        {
            if (!state.CanBalance)
                return 0;

            var (defaultQuantity, defaultPrice, limit) = GetParameters(state);
            for (var i = 1; i <= needDepth; i++)
            {
                defaultQuantity *= ((100m + config.MartinPercent) / 100).Round(state.CurrencyPair.AmountSignsNumber);
                limit += defaultQuantity * defaultPrice;
            }

            return limit;
        }


        private (decimal defaultQuantity, decimal defaultPrice, decimal limit) GetParameters(TradeState state)
        {
            decimal depo;
            decimal defaultQuantity;
            decimal firstBuyPrice;

            //если уже есть выставленные или купленные ордера, то за базовую цену считаем его
            var firstBuyOrder = state.GetFirstBuyOrder();
            if (firstBuyOrder != null)
            {
                depo = firstBuyOrder.Price * firstBuyOrder.OriginalQuantity;
                defaultQuantity = firstBuyOrder.OriginalQuantity;
                firstBuyPrice = firstBuyOrder.Price;
            }
            else
            {
                firstBuyPrice = state.ExchangeData.CurrentPrice;
                defaultQuantity = (config.DepositOrder / firstBuyPrice)
                    .Round(state.CurrencyPair.AmountSignsNumber);
                depo = state.ExchangeData.CurrentPrice * defaultQuantity;
                state.CalculatedDepositOrder = depo;
            }

            return (defaultQuantity, firstBuyPrice, depo);
        }
    }
}
