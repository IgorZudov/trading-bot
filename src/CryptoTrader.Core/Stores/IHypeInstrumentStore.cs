using System.Collections.Generic;
using CryptoTrader.Core.HypeAnalyzer;

namespace CryptoTrader.Core.Stores
{
    /// <summary>
    /// Хранилище сигналов
    /// </summary>
    public interface IHypeInstrumentStore
    {
        public void Save(List<HypePositionSignal> instruments);

        public List<HypePositionSignal> Get();

        public void Clear();
    }
}
