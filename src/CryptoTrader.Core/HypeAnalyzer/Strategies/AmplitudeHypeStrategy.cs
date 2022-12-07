using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Stores;
using CryptoTrader.Utils;
using CryptoTrader.Utils.Collection;
using CryptoTrader.Utils.Results;

namespace CryptoTrader.Core.HypeAnalyzer.Strategies
{
    public class AmplitudeHypeStrategy : IHypeStrategy
    {
        private readonly IExchangeClient client;
        private readonly ICheckInstrumentStore instrumentStore;
        private readonly IExchangeDataProvider dataProvider;
        private readonly HypeConfiguration config;

        private readonly ConcurrentDictionary<CurrencyPair, HypePositionSignal> signals =
            new();

        public AmplitudeHypeStrategy(IExchangeClient client, ICheckInstrumentStore instrumentStore,
            CoreConfiguration configuration, IExchangeDataProvider dataProvider)
        {
            this.client = client;
            config = configuration.HypeConfiguration;
            this.instrumentStore = instrumentStore;
            this.dataProvider = dataProvider;
        }

        public string Name => "AmplitudeHype";

        public async Task Update()
        {
            IEnumerable<CurrencyPair>? instruments = instrumentStore.Get();
            if (instruments == null || !instruments.Any())
                instruments = await client.GetExchangeInfo(config.BaseCurrency);

            foreach (var instrument in instruments)
            {
                var klines = await dataProvider.GetCandles(
                    new GetCandlesFilter
                    {
                        InstrumentId = instrument.InstrumentId,
                        Count = config.KlinesCount,
                        Interval = KlineInterval.OneMinute
                    });

                await BuildInfo(klines, instrument)
                    .OnSuccess(x =>
                    {
                        signals[x.Currency] = new HypePositionSignal
                        {
                            Priority = SignalPriority.Normal,
                            Pair = x.Currency,
                            Info = new SignalInfo(Name)
                            {
                                Amplitude = x.Amplitude,
                                Volume = x.Volume,

                                //todo сделать поумнее (смотреть процент последней свечи)
                                InstantBuy = false
                            }
                        };
                    });
            }

            async Task<Result<AmplitudeInstrumentInfo>> BuildInfo(ICollection<Kline> klines, CurrencyPair instrument)
            {
                //todo анализ объема стакана
                //TODO: проверять среднечасовой объем двух последних сессий? дабы исключить памп?
                //todo расширять конфигурацию на премаркете
                //todo убедиться, что свечи без пробелов
                //TODO: исключить свечи - проколы
                //todo: исключить  обвалы после отчета/открытия

                return await Result.Ok(klines)
                    .Ensure(k => k != null && k.Any())
                    .Ensure(k =>
                    {
                        //todo несколько запросов
                        var first = k.MaxBy(x => x.Time);
                        //исключаем те, у которых запаздывают свечи
                        return first != null && first.Time >= DateTime.UtcNow.AddMinutes(-15);
                    })
                    .Ensure(k =>
                    {
                        //вхождение в диапазон цены
                        var avgPrice = k.Average(_ => (_.High + _.Low) / 2);
                        return avgPrice >= config.MinPrice && avgPrice <= config.MaxPrice;
                    })
                    .Ensure(k =>
                    {
                        //количество нулевых свечей
                        var emptyFramesCount = k.Count(x => x.Amplitude <= 0.0001m);
                        return !(k.Count * 0.2 < emptyFramesCount);
                    })
                    .OnSuccess(k =>
                    {
                        return client.GetData(instrument.InstrumentId)
                            .Ensure(d => d.Available)
                            .Ensure(d => d.Spread <= config.MaxSpread) //проверяем максимальный спрэд
                            .Ensure(d => d.OrderbookDepth >= config.MinOrderbookDepth)
                            .Map(_ => k);
                    })
                    .OnSuccess(async k =>
                    {
                        var historyData = await dataProvider.GetCandles(new GetCandlesFilter
                        {
                            InstrumentId = instrument.InstrumentId,
                            Interval = KlineInterval.OneMinute,
                            From = DateTime.UtcNow.Date
                        });
                        if (!historyData.Any())
                            return Result.Ok(k);

                        var first = historyData.FirstOrDefault();
                        var last = historyData.LastOrDefault();

                        if (first == null || last == null)
                            return Result.Ok(k);

                        var maxDayPrice = historyData.Max(x => x.High);

                        //если за день отросли более чем на 10% и не было отката от максимумов на 3%
                        if (last.ClosePrice / first.OpenPrice > 1.1m && maxDayPrice / last.ClosePrice < 1.03m)
                            return new Error("За день отросли более чем на 10% и не было отката от максимумов на 3%");

                        //todo возможно стоит сделать проверку на неделю и рост на 30-40%
                        return Result.Ok(k);
                    })
                    .OnSuccess(k =>
                    {
                        var klinesParts = k.Split(5);
                        return CreateFrame(klinesParts, KlineInterval.FiveMinute).ToList();
                    })
                    .Ensure(f =>
                    {
                        var weekFrameCount = f.Count(x => x.Volume <= config.MinFrameVolume);
                        return !(f.Count * 0.2 <= weekFrameCount);
                    })
                    .Map(f => new AmplitudeInstrumentInfo
                    {
                        Volume = f.Sum(x => x.Volume),
                        AvgTrades = f.Average(_ => _.AvgTrades),
                        Amplitude = f.Average(_ => _.Amplitude),
                        Currency = instrument,
                        Name = instrument.Name,
                        LastCandleFalling = f.LastOrDefault()?.IsFalling ?? false
                    })
                    .Ensure(r => r.AvgTrades >= config.MinTrades);
            }

            IEnumerable<Frame> CreateFrame(IEnumerable<Kline[]> klinesParts, KlineInterval interval)
            {
                foreach (var klinesPart in klinesParts)
                {
                    var time = klinesPart.Min(x => x.Time);
                    var amplitude = klinesPart.Max(_ => _.High) / klinesPart.Min(_ => _.Low);
                    var avgPrice = klinesPart.Average(_ => (_.High + _.Low) / 2);
                    var avgTrades = klinesPart.Average(_ => _.Trades);
                    var haveDownTrend = false;
                    var closePrice = klinesPart.Last().ClosePrice;
                    var openPrice = klinesPart.First().OpenPrice;
                    var volume = klinesPart.Sum(x => x.Volume);
                    if (openPrice - closePrice > 0)
                    {
                        if (klinesPart.First().OpenPrice / klinesPart.Last().ClosePrice * 100 >=
                            config.DownTrendPercent)
                            haveDownTrend = true;
                    }

                    yield return new Frame(amplitude, avgPrice, avgTrades, haveDownTrend, volume, time, interval,
                        openPrice > closePrice);
                }
            }
        }

        public Result<List<HypePositionSignal>> GetSignals() =>
            signals.Values.OrderByDescending(x => x.Info.Amplitude)
                .ThenByDescending(x => x.Info.Volume)
                .ToList();
    }
}
