using CryptoTrader.Core.Models;
using LiteDB;

namespace CryptoTrader.Persist.Candles
{
    public class CandleModel
    {
        public string InstrumentId { get; set; }

        [BsonId]
        public MongoDB.Bson.ObjectId Id { get; set; }

        public Kline Candle { get; set; }

        public CandleModel()
        {
        }

        public CandleModel(Kline candle, string instrumentId)
        {
            InstrumentId = instrumentId;
            Candle = candle;
        }
    }
}
