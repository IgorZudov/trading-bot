using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTrader.Core.Helpers
{
    public static class CalculationHelper
    {
        public static decimal GetUnProfitPrice(decimal price, int roundSignNumber, decimal takeProfit) =>
            decimal.Round(price * 100 / (100 + takeProfit), roundSignNumber, MidpointRounding.AwayFromZero);

        public static decimal GetAveragePrice(params decimal[] prices)
        {
            var enumerable = prices as IList<decimal>;
            var sum = enumerable.Sum();
            return sum / enumerable.Count;
        }

        [Obsolete("Считаем профит в TradeState")]
        public static decimal GetProfitPrice(decimal price, int roundSignNumber, decimal takeProfit) =>
            decimal.Round(price * (takeProfit / 100 + 1), roundSignNumber, MidpointRounding.AwayFromZero);

        public static decimal Round(this decimal value, int roundSignNumber) =>
            decimal.Round(value, roundSignNumber, MidpointRounding.AwayFromZero);
    }
}
