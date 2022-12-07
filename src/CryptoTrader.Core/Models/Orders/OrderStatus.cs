namespace CryptoTrader.Core.Models.Orders
{
    public enum OrderStatus
    {
        New,
        PartiallyFilled,
        Filled,
        Canceled,
        PendingCancel,
        Rejected,
        Expired,
    }
}
