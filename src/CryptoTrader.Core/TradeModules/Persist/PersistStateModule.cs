using System.Threading.Tasks;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.TradeModules.Persist
{
    public class PersistStateModule : ITradingModule
    {
        private readonly ITradeStateStore store;

        public PersistStateModule(ITradeStateStore store)
        {
            this.store = store;
        }

        public ITradingModule Next { get; set; }

        public async Task ProcessState(TradeState state)
        {
            store.Save(state);

            if (Next != null)
                await Next.ProcessState(state);

            store.Save(state);
        }
    }
}
