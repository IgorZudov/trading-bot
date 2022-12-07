using System;
using System.Linq;
using Binance.Net.Objects;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Binance
{
    public static class BinanceSymbolMapper
    {
        public static CurrencyPair MapFromBinance(BinanceSymbol symbol) => new(symbol.Name,
            GetAmountSignNumbers(symbol), GetPriceSignNumbers(symbol));

        public static int GetPriceSignNumbers(BinanceSymbol symbol)
        {
            var filter = (BinanceSymbolPriceFilter) symbol.Filters.FirstOrDefault(symbolFilter =>
                symbolFilter is BinanceSymbolPriceFilter);
            var price = filter.MinPrice;

            if (price == 1)
                return 0;

            var signCounts = ConvertToSignCounts(price.ToString());
            if (signCounts == -1)
            {
                return 6;
            }

            return signCounts;
        }

        public static int GetAmountSignNumbers(BinanceSymbol symbol)
        {
            var filter =
                (BinanceSymbolLotSizeFilter) symbol.Filters.FirstOrDefault(symbolFilter =>
                    symbolFilter is BinanceSymbolLotSizeFilter);
            var amount = filter.MinQuantity;
            return amount == 1 ? 0 : ConvertToSignCounts(amount.ToString());
        }

        private static int ConvertToSignCounts(string str)
        {
            var comaIndex = str.IndexOf(".", StringComparison.Ordinal);
            var oneSignIndex = str.IndexOf("1", StringComparison.Ordinal);
            if (oneSignIndex == -1)
                return -1;
            return str.Substring(comaIndex + 1, oneSignIndex - comaIndex).Length;
        }
    }
}
