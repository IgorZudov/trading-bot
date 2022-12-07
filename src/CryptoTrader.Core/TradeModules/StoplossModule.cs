using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using Microsoft.Extensions.Logging;

namespace CryptoTrader.Core.TradeModules
{
    public class StoplossModule : ITradingModule
    {
        private readonly ILogger<StoplossModule> logger;

        public ITradingModule Next { get; set; }

        public List<(decimal, decimal)> Buys { get; } = new();

        public StoplossModule(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<StoplossModule>();
        }

        public async Task ProcessState(TradeState state)
        {
            logger.LogTrace(nameof(StoplossModule) + " Start");
            var buys = state.ActiveOrders
                .BuyOrders()
                .WithStatus(OrderStatus.Filled)
                .Select(order => (order.OriginalQuantity, order.Price));

            Buys.AddRange(buys);
            if (state.ActiveOrders.SellOrders().WithStatus(OrderStatus.Filled).Any())
            {
                Buys.Clear();
            }

            if (Buys.Count == state.MaxBuyDepth)
            {
                var lastSellOrder = state.ActiveOrders.FirstOrDefault(_ => _.Side == OrderSide.Sell);
                if (lastSellOrder == null)
                    return;

                var profitPrice = lastSellOrder.Price;
                Order newSellOrder;
                if (state.ExchangeData.CurrentPrice < profitPrice * (1 - 0.05m))
                {
                    state.CancelOrder(state.ActiveOrders.First(_ => _.Side == OrderSide.Sell).Id);
                    newSellOrder = OrderFactory.CreateSellOrder(state.CurrencyPair, lastSellOrder.OriginalQuantity,
                        state.ExchangeData.CurrentPrice, 0);
                    state.AddNewSellOrder(newSellOrder);
                    logger.LogTrace(nameof(StoplossModule) + " 0 profit");
                }
                else if (state.ExchangeData.CurrentPrice < profitPrice * (1 - 0.03m))
                {
                    var totalMainCoinQuantity = 0m;
                    var totalCoins = state.BuyedOrders.Sum(_ => _.OriginalQuantity);
                    foreach (var bougthOrder in state.BuyedOrders)
                    {
                        totalMainCoinQuantity += bougthOrder.Price * bougthOrder.OriginalQuantity;
                    }

                    var price = totalMainCoinQuantity / totalCoins;
                    state.CancelOrder(lastSellOrder.Id);
                    newSellOrder = OrderFactory.CreateSellOrder(state.CurrencyPair, lastSellOrder.OriginalQuantity,
                        decimal.Round(price, state.CurrencyPair.PriceSignsNumber, MidpointRounding.AwayFromZero), 0);
                    state.AddNewSellOrder(newSellOrder);
                    logger.LogTrace(nameof(StoplossModule) + " by current price");
                }
            }

            if (Next != null)
                await Next.ProcessState(state);
        }
    }
}
