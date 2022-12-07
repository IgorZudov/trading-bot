using System.Collections.Generic;
using CryptoTrader.Core.HypeAnalyzer;
using LiteDB;

namespace CryptoTrader.Persist.HypeInstrument
{
    public class HypeInstrumentModel
    {
        [BsonId]
        public int Id { get; } = HypeInstrumentStore.Id;

        public List<HypePositionSignal> Instruments { get; set; }

        public HypeInstrumentModel()
        {
        }

        public HypeInstrumentModel(List<HypePositionSignal> instruments)
        {
            Instruments = instruments;
        }
    }
}
