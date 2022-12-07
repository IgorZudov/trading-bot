using System;
using System.Collections.Generic;
using CryptoTrader.Core.Models;
using LiteDB;

namespace CryptoTrader.Persist.CheckInstrument
{
    public class CheckInstrumentModel
    {
        public DateTimeOffset ExpireAt { get; set; }

        [BsonId]
        public ObjectId Id { get; }

        public CurrencyPair Instrument { get; set; }

        public CheckInstrumentModel()
        {
        }

        public CheckInstrumentModel(CurrencyPair instrument)
        {
            Id = ObjectId.NewObjectId();
            Instrument = instrument;
            ExpireAt = DateTimeOffset.UtcNow.AddDays(10);
        }

        public bool IsExpired() => DateTimeOffset.UtcNow >= ExpireAt;
    }
}
