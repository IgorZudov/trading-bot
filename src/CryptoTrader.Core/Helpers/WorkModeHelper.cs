using System;
using CryptoTrader.Core.ExchangeWorkTime;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Exchange;

namespace CryptoTrader.Core.Helpers
{
    public static class WorkModeHelper
    {
        public static ExchangeWorkMode GetWorkMode(ExchangeType exchangeType) =>exchangeType switch
        {
            ExchangeType.Binance => ExchangeWorkMode.FullyWorks,
            ExchangeType.Tinkoff => TinkoffWorkingHours.GetWorkMode(DateTime.UtcNow)
        };
    }
}
