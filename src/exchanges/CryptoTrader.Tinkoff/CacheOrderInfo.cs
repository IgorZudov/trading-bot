using System.Collections.Generic;
using CryptoTrader.Utils;
using Tinkoff.Trading.OpenApi.Models;

namespace CryptoTrader.Tinkoff
{
    /// <summary>
    /// Кэш ордеров
    /// </summary>
    public class CacheOrderInfo
    {
        public string TickId { get; set; }

        public Result<List<Order>> OrderResult { get; set; }
    }
}
