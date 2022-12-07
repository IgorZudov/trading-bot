using System.ComponentModel;

namespace CryptoTrader.Core.Models.Exchange
{
    public enum ExchangeWorkMode
    {
        /// <summary>
        /// Полностью работает
        /// </summary>
        [Description("Полностью работает")]
        FullyWorks = 10,

        /// <summary>
        /// Закрыта
        /// </summary>
        [Description("Закрыта")]
        DoesntWork = 20,

        /// <summary>
        /// Без возможности выставлять ордера
        /// </summary>
        [Description("Постмаркет")]
        PostMarket = 30,

        [Description("Премаркет")]
        PreMarket = 40
    }
}
