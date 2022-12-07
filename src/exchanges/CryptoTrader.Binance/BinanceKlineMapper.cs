using System;
using Binance.Net.Objects;
using CryptoTrader.Core.Models;
using BinanceKlineInterval = Binance.Net.Objects.KlineInterval;
using KlineInterval = CryptoTrader.Core.Models.KlineInterval;

namespace CryptoTrader.Binance
{
    public static class BinanceKlineMapper
    {
        public static Kline MapFromBinance(BinanceKline kline) => new()
        {
            Low = kline.Low,
            Volume = kline.Volume,
            High = kline.High,
            Trades = kline.TradeCount,
            OpenPrice = kline.Open,
            ClosePrice = kline.Close
        };

        public static BinanceKlineInterval ToKlineInterval(KlineInterval side)
        {
            switch (side)
            {
                case KlineInterval.OneMinute:
                    return BinanceKlineInterval.OneMinute;

                case KlineInterval.FiveMinute:
                    return BinanceKlineInterval.FiveMinutes;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
    }
}
