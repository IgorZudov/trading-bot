using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Stores;

namespace CryptoTrader.Core
{
    public class ExchangeDataProvider : IExchangeDataProvider
    {
        private readonly IExchangeClient client;
        private readonly ICandleStore store;
        private const int CacheDayCount = 30;

        public ExchangeDataProvider(IExchangeClient client, ICandleStore store)
        {
            this.client = client;
            this.store = store;
        }

        public async Task<List<Kline>> GetCandles(GetCandlesFilter filter)
        {
            var candles = (await store.Get(filter)).OrderBy(x => x.Time).ToList();
            return candles;
        }

        public async Task<bool> IsDataActual(string instrumentId)
        {
            var  actualLimit = TimeSpan.FromHours(10);
            var last = await store.GetLast(instrumentId);
            if (last == null)
                return false;

            return DateTime.UtcNow - last.Time < actualLimit;
        }

        public async Task Save(string instrumentId, TimeSpan? limit = null)
        {
            var startOperationDate = DateTimeOffset.UtcNow;
            var startDate = startOperationDate.AddDays(-CacheDayCount);
            var last = await store.GetLast(instrumentId);

            if (last != null)
                startDate = last.Time;

            do
            {
                var now = DateTimeOffset.UtcNow;
                //если превысили лимит на загрузку
                if (limit.HasValue && now - startOperationDate > limit)
                    return;

                if (startDate >= now)
                    return;

                //инструмент делистился
                if (last != null && startDate - last.Time > TimeSpan.FromDays(55))
                    return;

                var to = startDate.AddDays(1);
                if (to > now)
                    to = now;

                //все текущие свечи актуальные
                if (to - startDate <= TimeSpan.FromMinutes(1) ||  DateTimeOffset.UtcNow - startDate <= TimeSpan.FromMinutes(1))
                    return;

                //todo не опрашивать апишку в выходной, если мы загрузили все исторические данные
                var result = await client.GetKlines(new GetCandlesFilter
                    {
                        From = startDate,
                        To = to,
                        InstrumentId = instrumentId,
                        Interval = KlineInterval.OneMinute
                    }, false, false);

                if (result == null || (!result.Any() && to.Date == now.Date))
                    return;

                result = result.OrderBy(x => x.Time).ToList();

                //в выходной придет одна свеча
                if (last != null && result.Count() == 1 && result.First().Time == last.Time)
                {
                    startDate = startDate.AddDays(1);
                    continue;
                }

                if(last != null)
                    result = result.Where(x => x.Time > last.Time).ToList();

                if (result.Any())
                {
                    foreach (var kline in result)
                        MarketWorkTime.FillKline(kline);
                    await store.Save(instrumentId, result.ToList());
                }
                startDate = startDate.AddDays(1);
            }
            while (true);
        }
    }
}
