using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Exchange;
using CryptoTrader.Core.Models.Orders;

namespace CryptoTrader.Core.TradeModules
{
    public class PostMarketModule : ITradingModule
    {
        public ITradingModule Next { get; set; }

        public async Task ProcessState(TradeState state)
        {
            if (OnPostMarket(state))
            {
                if (HandlePostMarket(state))
                    return;

                if (Next != null)
                    await Next.ProcessState(state);

                _ = HandlePostMarket(state);
                return;
            }

            if (Next != null)
                await Next.ProcessState(state);
        }

        private bool HandlePostMarket(TradeState state)
        {
            // false если мы не смогли подчистить стейт,
            // значит нужно еще прогнать одну итерацию,
            // чтобы получить актуальный стейт

            if (!state.ActiveOrders.Any())
                return true;

            // Что то купилось/продалось
            if (state.ActiveOrders.BuyOrders().IsInDeal() &&
                !state.ActiveOrders.SellOrders().Any())
                return false;

            var sellOrder = state.ActiveOrders.SingleOrDefault(o => o.Side == OrderSide.Sell);
            if (sellOrder != null)
            {
                sellOrder.CorrectSellOrderToNew();
                state.AddNewSellOrder(sellOrder);
            }

            state.CancelActiveOrders(false);
            return true;
        }

        /// <summary>
        /// На неработающей бирже также синхронизируем ордера
        /// </summary>
        private static bool OnPostMarket(TradeState state) => state.ExchangeWorkMode == ExchangeWorkMode.PostMarket ||
                                                              state.ExchangeWorkMode == ExchangeWorkMode.DoesntWork;
    }
}
