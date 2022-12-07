using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeJam.Collections;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Stores;
using MongoDB.Driver;

namespace CryptoTrader.Persist.Candles
{
    public class CandleStore : ICandleStore
    {
        private const string DbName = "ct-bot";
        private readonly IMongoDatabase db;

        public CandleStore(IMongoClient client)
        {
            db = client.GetDatabase(DbName);
        }

        public async Task Save(string instrument, List<Kline> klines)
        {
            if (klines == null || !klines.Any())
                return;

            var first = klines.FirstOrDefault();
            var from = first.Time;
            var collection = await GetCollection(instrument);

            var existed = collection.AsQueryable().Where(x => x.Candle.Time >= from).ToList();
            if (existed.Any())
                klines = klines.ExceptBy(existed.Select(x => x.Candle), x => x.Time).ToList();

            if (klines.Any())
                await collection.InsertManyAsync(klines.Select(x => new CandleModel(x, instrument)));
        }

        public async Task<List<Kline>> Get(GetCandlesFilter filter)
        {
            var collection = await GetCollection(filter.InstrumentId);
            IQueryable<CandleModel> query = collection.AsQueryable();

            if (filter.From.HasValue)
                query = query.Where(x => x.Candle.Time >= filter.From);

            if (filter.To.HasValue)
                query = query.Where(x => x.Candle.Time <= filter.To);

            if (filter.To is null && filter.From is null)
                query = query.OrderByDescending(x => x.Candle.Time);

            if(filter.Modes != null && filter.Modes.Any())
                query = query.Where(x => filter.Modes.Contains(x.Candle.MarketMode.Value));

            if (filter.Count.HasValue)
                query = query.Take(filter.Count.Value);

            return Enumerable.DistinctBy(query.ToList().Select(x => x.Candle), x => x.Time).ToList();
        }

        public async Task<Kline?> GetLast(string instrumentId)
        {
            var collection = await GetCollection(instrumentId);
            return collection.AsQueryable().OrderByDescending(x => x.Candle.Time).FirstOrDefault()?.Candle;
        }

        private async Task<IMongoCollection<CandleModel>> GetCollection(string instrumentId)
        {
            IMongoCollection<CandleModel> collection = null;

            var tables = (await db.ListCollectionNamesAsync()).ToList();
            if (tables.All(x => x != instrumentId))
            {
                await db.CreateCollectionAsync(instrumentId);
                collection = db.GetCollection<CandleModel>(instrumentId);
                //ttl на месяц
                await collection.Indexes.CreateOneAsync(Builders<CandleModel>.IndexKeys.Ascending(x => x.Candle.Time),
                    new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30), Name = $"ttl_{instrumentId}" });
            }
            if (collection == null)
                collection = db.GetCollection<CandleModel>(instrumentId);
            return collection;
        }
    }
}
