namespace CryptoTrader.Core.Models.Orders
{
    /// <summary>
    /// Модель растягивания выставления ордеров на покупку
    /// </summary>
    public class StretchingBuyOrdersOptions
    {
        /// <summary>
        /// Плюсовой шаг
        /// </summary>
        public decimal PlusStep { get; set; } = 0;

        /// <summary>
        /// С какого по счету ордера начинаем добавлять плюсовой шаг
        /// </summary>
        public int StartOrderNumber { get; set; } = 5;
    }
}
