using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Stores;
using CryptoTrader.Utils.Collection;
using LiteDB;

namespace CryptoTrader.Persist.CheckInstrument
{
    public class CheckInstrumentStore : ICheckInstrumentStore
    {
        private readonly LiteDatabase db;

        public CheckInstrumentStore(LiteDatabase context)
        {
            db = context;
        }

        public void Save(CurrencyPair instrument)
        {
            var collection = db.GetCollection<CheckInstrumentModel>();
            if (collection.Exists(
                x => x.Instrument.InstrumentId == instrument.InstrumentId))
                return;
            collection.Upsert(new CheckInstrumentModel(instrument));
        }


        public List<CurrencyPair> Get()
        {
            var models = db.GetCollection<CheckInstrumentModel>()
                .Query()
                .ToList();

            if (!models.Any())
                return null;

            var ready = models
                .Where(x => !x.IsExpired()).ToList();

            var toDelete = models.Except(ready).ToList();
            if (toDelete.Any())
                Delete(toDelete.Select(x => x.Instrument.InstrumentId).ToList());

            if (!ready.Any())
                return null;
            return ready.Select(x => x.Instrument).ToList();
        }

        private void Delete(IEnumerable<string> instrumentIds)
        {
            var collection = db.GetCollection<CheckInstrumentModel>();
            var batch = instrumentIds.Split(10);
            foreach (var ids in batch)
                collection.DeleteMany(x => ids.Contains(x.Instrument.InstrumentId));
        }

        public void Clear() => db.GetCollection<CheckInstrumentModel>().DeleteAll();
    }
}
