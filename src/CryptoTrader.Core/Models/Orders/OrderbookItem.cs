namespace CryptoTrader.Core.Models.Orders
{
    public class OrderbookItem
    {
        public int Quantity { get; }

        public decimal Price { get; }

        public OrderbookItem(decimal price, int quantity)
        {
            Price = price;
            Quantity = quantity;
        }
    }
}
