using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.HypeAnalyzer;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.TradeModules
{
    public class HypeModule : ITradingModule
    {
        private readonly IHypePosition hypePosition;

        public ITradingModule? Next { get; set; }

        public HypeModule(IHypePosition hypePosition)
        {
            this.hypePosition = hypePosition;
        }

        public async Task ProcessState(TradeState state)
        {
            if (!state.CanChangeInstrument() && Next != null)
            {
                await Next.ProcessState(state);
                return;
            }

            var hypeCurrency = await hypePosition.GetPosition(state.Id);
            if (hypeCurrency == null && state.CurrencyPair != null)
                //перестаем работать, если нет хайпового инструмента
                await CheckIdleState();

            if (hypeCurrency == null)
                return;

            if (!Equals(hypeCurrency.Pair, state.CurrencyPair))
            {
                if (state.CanChangeInstrument())
                {
                    state.CurrencyPair = hypeCurrency.Pair;
                    state.SignalInfo = hypeCurrency.Info;
                    state.ResetStatistic();
                    return;
                }

                await CheckIdleState();
            }
            else
            {
                state.SignalInfo = hypeCurrency.Info;
                if (Next != null)
                    await Next.ProcessState(state);
            }

            async Task CheckIdleState()
            {
                if (IsCanCancelCurrentOrders(state))
                {
                    state.CancelActiveOrders();
                    state.CurrencyPair = null;
                }
                else
                {
                    if (Next != null)
                        await Next.ProcessState(state);
                }
            }
        }

        private static bool IsCanCancelCurrentOrders(TradeState state) =>
            !state.ActiveOrders.SellOrders().Any() &&
            state.ActiveOrders.IsNotInDeal() &&
            state.PartialCoinsAmount == 0 &&
            state.NewOrders.Count == 0;
    }
}
