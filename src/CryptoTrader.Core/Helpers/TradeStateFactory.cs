using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.HypeAnalyzer;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Helpers
{
    public static class TradeStateFactory
    {
        //todo igor: покрыть тестами инициализацию стейтов
        public static IEnumerable<TradeState> CreateStates(CoreConfiguration config,
            List<TradeState>? existedStates, List<HypePositionSignal>? hypeInstruments)
        {
            var needStateCount = config.MaxStateCount;

            if (existedStates == null || !existedStates.Any())
                return CreateInternal(config.MaxStateCount);

            if (existedStates.Count >= needStateCount)
                return existedStates;

            if (hypeInstruments != null && hypeInstruments.Any())
            {
                var existedInstrumentIds = existedStates
                    .Select(x => x.CurrencyPair.InstrumentId.ToString())
                    .ToList();
                hypeInstruments = hypeInstruments.Where(x => !existedInstrumentIds.Contains(x.Pair.InstrumentId))
                    .ToList();
            }

            return CreateInternal(config.MaxStateCount - existedStates.Count);

            IEnumerable<TradeState> CreateInternal(int count) =>
                Enumerable.Range(0, count).Select(x =>
                {
                    HypePositionSignal? signal = null;
                    if (hypeInstruments != null && hypeInstruments.Count - 1 >= x)
                        signal = hypeInstruments[x];
                    return new TradeState(config, signal?.Pair) { SignalInfo = signal?.Info };
                });
        }
    }
}
