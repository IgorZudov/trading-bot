using System;
using System.Collections.Generic;
using CryptoTrader.Core.Models.Orders;

namespace CryptoTrader.Fake
{
    internal class HistoricalDataProvider : IDisposable
    {
        public void Dispose()
        {
        }


        public IEnumerable<Order> GetPrices(string currencyCryptoPair, DateTime start, DateTime end)
        {
            return new Order[0]; //todo Реализовать исторический провайдер
        }
    }
}
