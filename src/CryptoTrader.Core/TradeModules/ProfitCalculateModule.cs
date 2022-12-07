using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;

namespace CryptoTrader.Core.TradeModules
{
    public class ProfitCalculateModule : ITradingModule
    {
        private readonly CoreConfiguration configuration;
        private readonly ProfitKeeper dealLogger = ProfitKeeper.Default;

        public ITradingModule Next { get; set; }

        public ProfitCalculateModule(CoreConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Task ProcessState(TradeState state)
        {
            var filledSellOrder = state.ActiveOrders.FirstOrDefault(order => order.Side == OrderSide.Sell
                                                                             && order.Status == OrderStatus.Filled);
            if (filledSellOrder != null)
            {
                if (dealLogger.Deals.Count == 0)
                {
                    var tempPrice = filledSellOrder.GetLastPriceWithoutProfit(state.TakeProfit);
                    var profit = filledSellOrder.OriginalQuantity * (filledSellOrder.Price - tempPrice);
                    var roundProfit = decimal.Round(profit, state.CurrencyPair.PriceSignsNumber,
                        MidpointRounding.AwayFromZero);
                    dealLogger.Deals.Add(new Deal(profit, roundProfit));
                }
                else
                {
                    var tempPrice = filledSellOrder.GetLastPriceWithoutProfit(state.TakeProfit);
                    var profit = filledSellOrder.OriginalQuantity * (filledSellOrder.Price - tempPrice);
                    var roundProfit = decimal.Round(profit, state.CurrencyPair.PriceSignsNumber,
                        MidpointRounding.AwayFromZero);
                    dealLogger.Deals.Add(new Deal(profit, dealLogger.Deals.Last().TotalProfit + roundProfit));
                }
                dealLogger.Save();
            }
            return Task.CompletedTask;
        }
    }
}
