using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Orders;

namespace CryptoTrader.Core.Models
{
    public class ExchangeData
    {
        public decimal CurrentPrice { get; set; }

        public bool Available { get; set; }

        public List<OrderbookItem>? Bids { get; set; }

        public List<OrderbookItem>? Asks { get; set; }

        public decimal BidPrice => Bids?
            .OrderBy(x => x.Price)
            .Select(x => x.Price)
            .FirstOrDefault() ?? 0;

        public decimal AskPrice => Asks?.OrderByDescending(x => x.Price)
            .Select(x => x.Price)
            .FirstOrDefault() ?? 0;

        public decimal Spread
        {
            get
            {
                if (BidPrice == 0 || AskPrice == 0)
                    return 0;
                return (BidPrice / AskPrice - 1) * 100;
            }
        }

        public int OrderbookDepth => Asks != null && Bids != null ? (Asks.Count + Bids.Count) / 2 : 0;
    }
}
