using JetBrains.Annotations;

namespace CryptoTrader.Core.Models
{
    public class CurrencyPair
    {
        /// <summary>
        ///     Инструмент
        /// </summary>
        [UsedImplicitly]
        public string InstrumentId { get; set; }

        /// <summary>
        ///     Кол-во знаков после запятой на минимальную покупку валюты
        /// </summary>
        [UsedImplicitly]
        public int AmountSignsNumber { get; set; }

        /// <summary>
        /// Наименование инструмента
        /// </summary>
        [UsedImplicitly]
        public string Name { get; set; }
        /// <summary>
        ///     Кол-во знаков после запятой на минимальную цену валюты
        /// </summary>
        [UsedImplicitly]
        public int PriceSignsNumber { get; set; }

        [UsedImplicitly]
        public CurrencyPair()
        {
        }

        public CurrencyPair(string instrumentId, int amountSignsNumber, int priceSignsNumber)
        {
            InstrumentId = instrumentId;
            AmountSignsNumber = amountSignsNumber;
            PriceSignsNumber = priceSignsNumber;
        }

        public override bool Equals(object obj)
        {
            if (obj is CurrencyPair pair)
            {
                return Equals(pair);
            }
            return false;
        }

        private bool Equals(CurrencyPair other)
        {
            return string.Equals(InstrumentId, other.InstrumentId) && AmountSignsNumber == other.AmountSignsNumber &&
                   PriceSignsNumber == other.PriceSignsNumber;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = (InstrumentId != null ? InstrumentId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ AmountSignsNumber;
                hashCode = (hashCode * 397) ^ PriceSignsNumber;
                return hashCode;
            }
        }

        public override string ToString() => Name;
    }
}
