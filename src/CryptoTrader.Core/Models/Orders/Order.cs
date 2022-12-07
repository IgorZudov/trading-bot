using System;

namespace CryptoTrader.Core.Models.Orders
{
    public class Order
    {
        public string Id { get; set; }

        public OrderSide Side { get; set; }

        public OrderStatus Status { get; set; }

        public decimal Price { get; set; }

        /// <summary>
        /// Количество исполненных лотов
        /// </summary>
        public decimal ExecutedQuantity { get; set; }

        /// <summary>
        ///  Тип ордера - лимита или по рынку
        /// </summary>
        public OrderType Type { get; set; }

        public TimeInForce TimeInForce { get; set; }

        /// <summary>
        /// Количество лотов
        /// </summary>
        public decimal OriginalQuantity { get; set; }

        public string Symbol { get; set; }

        /// <summary>
        /// Процент комиссионных в целых значениях
        /// например, 5% или 0.025%
        /// </summary>
        public decimal FeePercent { get; set; } = 0.025m;//todo подумать, куда перенести ставку комиссии

        /// <summary>
        /// Сумма комиссии
        /// </summary>
        public decimal Fee => FeePercent / 100 * ExecutedDeposit;

        /// <summary>
        /// Исполненная сумма
        /// </summary>
        public decimal ExecutedDeposit => ExecutedQuantity * Price;

        public override string ToString() =>
            $"{Side}: {Status} : Price - {Price} : Am - {OriginalQuantity}";

        public decimal GetLastPriceWithoutProfit(decimal profit) =>
            Price / (1 + profit / 100);


        /// <summary>
        /// Корректирует частичные суммы ордера на продажу,
        /// чтобы перевыставить его
        /// </summary>
        public void CorrectSellOrderToNew()
        {
            if (Side == OrderSide.Buy)
                throw new ApplicationException("Корректировке подвержены только sell ордера");

            if(ExecutedQuantity == 0)
                return;

            OriginalQuantity -= ExecutedQuantity;
            ExecutedQuantity = 0;
        }
    }
}
