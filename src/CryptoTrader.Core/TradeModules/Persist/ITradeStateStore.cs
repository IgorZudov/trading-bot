using System.Collections.Generic;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.TradeModules.Persist
{
    public interface ITradeStateStore
    {
        public void Save(TradeState state);

        public TradeState Get(string id);

        public List<TradeState> GetAll();

        public void Delete(string id);

        public void Clear();
    }
}
