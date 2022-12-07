using System;

namespace CryptoTrader.Core.Models
{
    public class Deal
    {
        public string Time { get; set; }
        public decimal Profit { get; set; }
        public decimal TotalProfit { get; set; }


        public Deal()
        {
        }

        public Deal(decimal profit, decimal totalProfit)
        {
            Time = DateTime.UtcNow.ToString("G");
            Profit = profit;
            TotalProfit = totalProfit;
        }
    }
}
