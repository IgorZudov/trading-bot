using System.Collections.Generic;
using System.Linq;
using CodeJam.Strings;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.TradeModules.Persist;
using LiteDB;

namespace CryptoTrader.Persist.State
{
    public class TradeStateStore : ITradeStateStore
    {
        private readonly LiteDatabase db;
        private readonly CoreConfiguration configuration;

        public TradeStateStore(LiteDatabase context, CoreConfiguration configuration)
        {
            db = context;
            this.configuration = configuration;
        }

        public void Save(TradeState state)
        {
            var collection = db.GetCollection<StatePersistModel>();
            var persistModel = StatePersistModel.FromState(state);
            if (state.Id.IsNullOrEmpty())
            {
                collection.Insert(persistModel);
                state.Id = persistModel.Id.ToString()!;
            }
            else
            {
                collection.Update(persistModel);
            }
        }

        public TradeState Get(string id)
        {
            var objId = new ObjectId(id);
            var model = db.GetCollection<StatePersistModel>().FindOne(x => x.Id == objId);
            if (model == null)
                return null;

            return StatePersistModel.ToState(model, configuration);
        }

        public List<TradeState> GetAll()
        {
            return db.GetCollection<StatePersistModel>().FindAll()
                .Select(model => StatePersistModel.ToState(model, configuration))
                .ToList();
        }

        public void Delete(string id) => db.GetCollection<StatePersistModel>().Delete(new ObjectId(id));

        public void Clear() => db.GetCollection<StatePersistModel>().DeleteAll();
    }
}
