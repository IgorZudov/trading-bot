using System;

namespace CryptoTrader.Core.Models
{
    public class Frame
    {
        public decimal Amplitude { get; }

        public decimal AvgPrice { get; }

        public double AvgTrades { get; }

        public bool HaveDownTrend { get; }

        public decimal Volume { get; set; }

        public DateTimeOffset Time { get; set; }

        public KlineInterval Interval { get; set; }

        public bool IsFalling { get; }

        public bool IsGrowing => !IsFalling;

        public Frame(decimal amplitude, decimal avgPrice, double avgTrades,
            bool haveDownTrend, decimal volume, DateTimeOffset time,
            KlineInterval interval, bool isFalling)
        {
            Amplitude = amplitude;
            AvgPrice = avgPrice;
            AvgTrades = avgTrades;
            HaveDownTrend = haveDownTrend;
            Volume = volume;
            Time = time;
            Interval = interval;
            IsFalling = isFalling;
        }
    }
}
