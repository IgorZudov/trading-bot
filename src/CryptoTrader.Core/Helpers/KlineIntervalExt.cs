using System;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Helpers
{
    public static class KlineIntervalExt
    {
        public static TimeSpan ToTime(this KlineInterval interval) => interval switch
        {
            KlineInterval.OneMinute => TimeSpan.FromMinutes(1),
            KlineInterval.FiveMinute => TimeSpan.FromMinutes(5),
            _ => throw new ArgumentException()
        };
    }
}
