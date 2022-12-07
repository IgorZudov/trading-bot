namespace CryptoTrader.Core.Models
{
    /// <summary>
    /// Тип рынка
    /// </summary>
    public enum MarketMode
    {
        /// <summary>
        /// Неизвестно
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Премаркет
        /// </summary>
        PreMarket = 10,

        /// <summary>
        /// Рыночная сессия
        /// </summary>
        Market = 20,

        /// <summary>
        /// Постмаркет
        /// </summary>
        PostMarket = 30
    }
}
