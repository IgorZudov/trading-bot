using System;

namespace CryptoTrader.Core.Models
{
    /// <summary>
    /// Рабочее время рынка
    /// </summary>
    public static class MarketWorkTime
    {
        //todo сделать зависимости от летнего/зимнего времени

        //установлено летнее время
        public static TimeSpan PreMarketOpenUtcTime = new(4, 0, 0);

        public static TimeSpan MarketUtcTime = new(13, 30, 0);

        public static TimeSpan PostMarketUtcTime = new(20, 00, 0);

        public static TimeSpan PostMarketCloseUtcTime = new(22, 40, 0);

        /// <summary>
        /// Указываем режим работы рынка свече
        /// </summary>
        public static void FillKline(Kline kline)
        {
            var time = kline.Time.TimeOfDay;
            if (time >= PreMarketOpenUtcTime && time < MarketUtcTime)
                kline.MarketMode = MarketMode.PreMarket;
            else if (time >= MarketUtcTime && time < PostMarketUtcTime)
                kline.MarketMode = MarketMode.Market;
            else if (time >= PostMarketUtcTime && time <= PostMarketCloseUtcTime)
                kline.MarketMode = MarketMode.PostMarket;
            else
                kline.MarketMode = MarketMode.Unknown;
        }
    }
}
