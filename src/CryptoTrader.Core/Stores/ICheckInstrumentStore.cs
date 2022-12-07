using System.Collections.Generic;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Stores
{
    /// <summary>
    /// Хранилище списка инструментов для анализа и загрузки свечей
    /// </summary>
    public interface ICheckInstrumentStore
    {
        void Save(CurrencyPair instrument);
        List<CurrencyPair>? Get();
        void Clear();
    }
}
