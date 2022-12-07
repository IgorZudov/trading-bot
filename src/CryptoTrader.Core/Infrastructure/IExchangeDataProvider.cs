using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Infrastructure
{
    /// <summary>
    /// Поставщик биржевых данных
    /// </summary>
    public interface IExchangeDataProvider
    {
        Task<List<Kline>> GetCandles(GetCandlesFilter filter);

        /// <summary>
        /// Загружаем свечи c возможным временным лимитом на операцию
        /// </summary>
        Task Save(string instrumentId, TimeSpan? limit = null);

        /// <summary>
        /// Данные по инструменту актуальные (работает только во время работы биржи)
        /// </summary>
        //todo сделать проверки на выходные
        Task<bool> IsDataActual(string instrumentId);
    }

    public class GetCandlesFilter
    {
        public string InstrumentId { get; set; }

        public KlineInterval Interval { get; set; }

        public int? Count { get; set; }

        public DateTimeOffset? From { get; set; }

        public DateTimeOffset? To { get; set; }

        public List<MarketMode> Modes { get; set; }
    }
}
