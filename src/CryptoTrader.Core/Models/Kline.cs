using System;

namespace CryptoTrader.Core.Models
{
    /// <summary>
    /// Свеча
    /// </summary>
    public class Kline
    {
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public int Trades { get; set; }
        public decimal Volume { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public DateTimeOffset Time { get; set; }

        public KlineInterval Interval { get; set; }

        /// <summary>
        /// Тип рынка
        /// </summary>
        public MarketMode? MarketMode { get; set; }

        public decimal Amplitude => Math.Abs(OpenPrice - ClosePrice);

        /// <summary>
        /// Падающая
        /// </summary>
        public bool IsFalling => OpenPrice > ClosePrice;

        /// <summary>
        /// Растущая
        /// </summary>
        public bool IsGrowing => !IsFalling;
    }
}
