using System.Collections.Generic;
using CryptoTrader.Core.HypeAnalyzer;
using CryptoTrader.Core.Stores;
using LiteDB;

namespace CryptoTrader.Persist.HypeInstrument
{
    public class HypeInstrumentStore : IHypeInstrumentStore
    {
        internal const int Id = 1;
        private readonly LiteDatabase db;

        public HypeInstrumentStore(LiteDatabase context)
        {
            db = context;
        }

        public void Save(List<HypePositionSignal> instruments) => db.GetCollection<HypeInstrumentModel>()
            .Upsert(new HypeInstrumentModel(instruments));

        public List<HypePositionSignal> Get() =>
            db.GetCollection<HypeInstrumentModel>()
                .FindOne(s => s.Id == Id)?.Instruments;

        public void Clear() => db.GetCollection<HypeInstrumentModel>().DeleteAll();
    }
}
