namespace CryptoTrader.Core.Models
{
    public class AmplitudeInstrumentInfo
    {
        public decimal Volume { get; set; }

        public double AvgTrades { get; set; }

        public decimal Amplitude { get; set; }

        /// <summary>
        /// Последняя свеча красная
        /// </summary>
        public bool LastCandleFalling { get; set; }
        public CurrencyPair Currency { get; set; }

        public string Name { get; set; }
        public override string ToString() => $"{Currency.InstrumentId}: Amplitude: {Amplitude}";
    }
}
