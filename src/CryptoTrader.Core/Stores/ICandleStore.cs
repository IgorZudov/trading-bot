using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Stores
{
    public interface ICandleStore
    {
        Task Save(string instrument, List<Kline> klines);

        Task<List<Kline>> Get(GetCandlesFilter filer);

        /// <summary>
        /// Получить последнюю свечу
        /// </summary>
        Task<Kline?> GetLast(string instrumentId);
    }
}
