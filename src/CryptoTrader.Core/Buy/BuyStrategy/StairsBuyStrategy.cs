using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;

namespace CryptoTrader.Core.Buy.BuyStrategy
{
    /// <summary>
    /// Покупка лесенкой
    /// </summary>
    public class StairsBuyStrategy : IBuyOrderStrategy
    {
        private readonly CoreConfiguration config;
        private readonly StretchingBuyOrdersOptions buyOrdersOptions;

        public StairsBuyStrategy(CoreConfiguration config, StretchingBuyOrdersOptions buyOrdersOptions)
        {
            this.config = config;
            //перенести данный конфиг в state
            this.buyOrdersOptions = buyOrdersOptions;
        }

        public Task CreateBuyOrders(TradeState state)
        {
            var currentPrice = state.ExchangeData.CurrentPrice;
            while (state.CanAddNewBuyOrder())
            {
                decimal nextOrderPrice;
                decimal nextAmount;
                if (state.IsBuyOrdersExist())
                {
                    var lastOrders = state.GetLastBuyOrders(2).OrderByDescending(x => x.Price).ToArray();

                    decimal lastPercent;
                    if(lastOrders.Length == 2)
                        lastPercent = lastOrders[0].Price / lastOrders[1].Price * 100 - 100;
                    else
                        lastPercent = 0;

                    var lastOrder = lastOrders.Last();
                    var buyOrdersCount = state.GetBuyOrdersCount();

                    var percent = buyOrdersCount >= buyOrdersOptions.StartOrderNumber ?
                            lastPercent + buyOrdersOptions.PlusStep :
                            config.PercentStep;

                    nextOrderPrice = (100 - percent) / 100 * lastOrder.Price;
                    nextAmount = lastOrder.OriginalQuantity + lastOrder.OriginalQuantity * config.MartinPercent / 100;
                }
                else
                {
                    if (state.ActiveOrders.SellOrders().Any())
                        return Task.CompletedTask;

                    state.BuyOrdersPrice = currentPrice;
                    var deviation = state.InstantFirstBuy ? 0 : state.FirstStepDeviation;
                    nextOrderPrice = state.BuyOrdersPrice * (100 - deviation) / 100;
                    nextAmount = state.CalculatedDepositOrder / currentPrice;
                    state.LastDealSetTime = DateTime.UtcNow;
                }

                //в случае, если был простой работы бота
                if (nextOrderPrice > currentPrice)
                    nextOrderPrice = currentPrice;

                var order = OrderFactory.CreateBuyOrder(state.CurrencyPair, nextAmount,
                    nextOrderPrice);
                state.AddNewBuyOrder(order);
            }
            return Task.CompletedTask;
        }
    }
}
