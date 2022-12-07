using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.HypeAnalyzer.Strategies;
using CryptoTrader.Core.Stores;
using CryptoTrader.Utils.Results;

namespace CryptoTrader.Core.HypeAnalyzer
{
    public class HypeAnalyzer : IHypePosition
    {
        private readonly IHypeInstrumentStore store;
        private readonly IReadOnlyCollection<IHypeStrategy> strategies;

        private readonly Dictionary<string, HypePositionSignal> selectedInstruments = new();

        public HypeAnalyzer(IHypeInstrumentStore store,
            IReadOnlyCollection<IHypeStrategy> strategies)
        {
            this.store = store;
            this.strategies = strategies;
        }

        private void Update()
        {
            var signals = new List<HypePositionSignal>();
            foreach (var strategy in strategies)
                strategy.GetSignals()
                    .OnSuccess(s => signals.AddRange(s));

            //заполняем хайповые позици в моменте
            Positions = signals
                //todo filter strategies
                .OrderByDescending(x => x.Priority)
                .ToList();
            store.Save(Positions);
        }


        public Task<HypePositionSignal?> GetPosition(string id)
        {
            Update();
            HypePositionSignal? instrument = null;
            if (!selectedInstruments.ContainsKey(id))
                selectedInstruments[id] = null;
            else
                instrument = selectedInstruments[id];

            if (instrument != null &&
                Positions.Any(x => x.Pair.InstrumentId == instrument.Pair.InstrumentId))
                return Task.FromResult(instrument);

            var selectedIds = selectedInstruments.Values
                .Where(x => x != null)
                .Select(x => x.Pair.InstrumentId).ToArray();

            instrument = Positions
                .FirstOrDefault(x => !selectedIds.Contains(x.Pair.InstrumentId));

            selectedInstruments[id] = instrument;
            return Task.FromResult(instrument);
        }

        public List<HypePositionSignal> Positions { get; private set; } = new();
    }
}
