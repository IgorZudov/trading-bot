using ValueOf;

namespace CryptoTrader.Tinkoff
{
    public class ExchangeRequest : ValueOf<string, ExchangeRequest>
    {
        public static ExchangeRequest MarketRequest => From("Market");

        public static ExchangeRequest OrdersCancelRequest => From("OrdersCancel");

        public static ExchangeRequest OrdersRequest => From("Orders");

        public static ExchangeRequest PlaceLimitOrderRequest => From("PlaceLimitOrder");

        public static ExchangeRequest OrdersHistoryRequest => From("OrdersHistory");

        public static implicit operator string(ExchangeRequest ex) => ex.Value;

    }
}
