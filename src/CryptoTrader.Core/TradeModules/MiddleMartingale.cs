using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Buy.BuyStrategy;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Helpers;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;

namespace CryptoTrader.Core.TradeModules
{
    public class MiddleMartingale : ITradingModule
    {
        private readonly CoreConfiguration config;
        private readonly IBuyOrderStrategy buyStrategy;

        private TradeState currentState;

        public ITradingModule Next { get; set; }

        public MiddleMartingale(CoreConfiguration config, IBuyOrderStrategy buyStrategy)
        {
            this.config = config;
            this.buyStrategy = buyStrategy;
        }

        public async Task ProcessState(TradeState state)
        {
            currentState = state;
            await ResolveOrders();
            if (Next != null)
                await Next.ProcessState(state);
        }

        private async Task ResolveOrders()
        {
            // ReSharper disable AssignNullToNotNullAttribute
            currentState
                // случай когда ресторились из бд
                .IfHasNewSellOrder(CorrectSellIfPriceChanged)
                .IfBuyOrdersBoughtWhenSellOrderNotExist(CreateFirstSellOrder)
                .IfActiveSellOrdersNotExist(CancelBuyOrdersIfDeviationChange)
                .IfBuyOrdersBoughtWhenSellOrderExist(CancelCurrentSellOrderAndCreateNext)
                .IfSellOrderComplited(CancelAllOrders);

            //После всех шагов нужно создать новые ордера на покупку
            currentState.CalculateMaxBuyDepth();
            await buyStrategy.CreateBuyOrders(currentState);
        }

        /// <summary>
        ///         Отменяем текущий ордер на продажу и создаем новый с учетом профита
        /// </summary>
        /// <param name="sellOrder">Текущий ордер на продажу</param>
        /// <param name="buyedOrders">Ордера которые купились</param>
        private void CancelCurrentSellOrderAndCreateNext(Order sellOrder, ICollection<Order> buyedOrders)
        {
            Order newSellOrder = null;
            foreach (var buyedOrder in buyedOrders)
            {
                decimal profitPrice;
                decimal quantity;
                currentState.BuyedOrders.Add(buyedOrder);

                //todo здесь надо как-то разделить на три внутренних кейса (возможно больше)

                //todo Sell ордер продался вместе с покупкой Buy ордеров
                if (sellOrder.Status == OrderStatus.Filled)
                {
                    profitPrice = currentState.GetTakeProfitPrice();

                    if (newSellOrder == null)
                        quantity = buyedOrder.OriginalQuantity;
                    //todo не покрыто тестами
                    else
                        quantity = newSellOrder.OriginalQuantity + buyedOrder.OriginalQuantity;

                    newSellOrder = CreateSellOrderInternal(quantity, profitPrice);
                    continue;
                }

                //Sell ордер частично продался вместе с покупкой Buy ордеров
                if (sellOrder.Status == OrderStatus.PartiallyFilled)
                {
                    profitPrice = currentState.GetTakeProfitPrice();
                    if (newSellOrder == null)
                        quantity = buyedOrder.OriginalQuantity + (sellOrder.OriginalQuantity - sellOrder.ExecutedQuantity);
                    // todo не покрыто тестами
                    else
                        quantity = newSellOrder.OriginalQuantity + buyedOrder.OriginalQuantity;

                    newSellOrder = CreateSellOrderInternal(quantity, profitPrice);
                    continue;
                }

                //Sell ордер не изменился, пересоздаем его
                profitPrice = currentState.GetTakeProfitPrice();

                if (newSellOrder == null)
                    quantity = buyedOrder.OriginalQuantity + sellOrder.OriginalQuantity;
                else
                    quantity = buyedOrder.OriginalQuantity + newSellOrder.OriginalQuantity;

                newSellOrder = CreateSellOrderInternal(quantity, profitPrice);
            }
            currentState.CancelOrder(sellOrder.Id);
            currentState.AddNewSellOrder(newSellOrder ?? throw new ArgumentException());
        }

        private Order CreateSellOrderInternal(decimal quantity, decimal price)
        {
            var order = OrderFactory.CreateSellOrder(currentState.CurrencyPair,
                quantity, price,
                currentState.PartialCoinsAmount);
            currentState.PartialCoinsAmount = 0;
            return order;
        }

        private void CancelAllOrders(Order complitedSellOrder)
        {
            currentState.CancelActiveOrders();
        }

        private void CreateFirstSellOrder(ICollection<Order> buyedOrders)
        {
            Order sellOrder = null;
            foreach (var order in buyedOrders)
            {
                currentState.BuyedOrders.Add(order);
                var profitPrice = currentState.GetTakeProfitPrice();
                decimal quantity;

                if (sellOrder == null)
                    quantity = order.OriginalQuantity;
                else
                    quantity = order.OriginalQuantity + sellOrder.OriginalQuantity;
                sellOrder = CreateSellOrderInternal(quantity, profitPrice);
            }

            currentState.AddNewSellOrder(sellOrder ?? throw new ArgumentException());
        }

        /// <summary>
        ///         Пересоздаем ордера на покупку, если цена поднялась на определенный коэф.
        ///         Выполняется только если все ордера на покупку
        /// </summary>
        private void CancelBuyOrdersIfDeviationChange()
        {
            var currentPrice = currentState.ExchangeData.CurrentPrice;
            if (currentState.BuyOrdersPrice != 0)
            {
                var deviation =
                    decimal.Round(currentPrice / currentState.BuyOrdersPrice,
                        currentState.CurrencyPair.PriceSignsNumber, MidpointRounding.AwayFromZero) - 1;
                if (deviation > 0.05m) return; //если слишком прыгнула цена, то не перезагружаемся
                if (deviation >= (config.ReloadOrdersPercent / 100))
                {
                    currentState.CancelActiveOrders();
                }
            }
        }

        private void CorrectSellIfPriceChanged(Order order)
        {
            //выставляемся выше, если цена выше желаемой
            if (order.Price < currentState.ExchangeData.CurrentPrice)
                order.Price = currentState.ExchangeData.CurrentPrice;
        }
    }
}
