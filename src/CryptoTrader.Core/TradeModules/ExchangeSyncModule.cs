using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Notification;
using CryptoTrader.Core.Models.Orders;
using CryptoTrader.Core.TradeModules.Persist;
using Microsoft.Extensions.Logging;

namespace CryptoTrader.Core.TradeModules
{
    public class ExchangeSyncModule : ITradingModule
    {
        private readonly ILogger logger;
        private readonly IExchangeClient client;
        private readonly INotificationService notificationService;
        private readonly ITradeStateStore tradeStateStore;

        public ITradingModule Next { get; set; }

        public ExchangeSyncModule(IExchangeClient client, ILoggerFactory loggerFactory,
            INotificationService notificationService, ITradeStateStore tradeStateStore)
        {
            this.client = client;
            this.notificationService = notificationService;
            this.tradeStateStore = tradeStateStore;
            logger = loggerFactory.CreateLogger<ExchangeSyncModule>();
        }

        public async Task ProcessState(TradeState state)
        {
            await UpdateState(state);

            if (Next != null)
                await Next.ProcessState(state);

            await PostState(state);
        }

        private async Task UpdateState(TradeState state)
        {
            if (state.CurrencyPair == null)
                return;

            var getDataResult = await client.GetData(state.CurrencyPair.InstrumentId);
            if (getDataResult.Success)
                state.ExchangeData = getDataResult.Value;

            await client.UpdateOrders(state.CurrencyPair.InstrumentId, state.ActiveOrders, state.TickId);
            logger.LogInformation(state.ToString());
        }

        private async Task PostState(TradeState tradeState)
        {
            var success = await CancelOrders(tradeState);
            if (!success)
            {
                tradeState.ActiveOrders.RemoveAll(order => order.Status == OrderStatus.Filled);
                tradeState.ActiveOrders.AddRange(tradeState.CancelOrders);
                tradeState.NewOrders.Clear();
                tradeState.CancelOrders.Clear();
                return;
            }

            tradeState.ActiveOrders.RemoveAll(order => order.Status == OrderStatus.Filled);
            await PlaceOrders(tradeState);

            tradeState.CancelOrders.Clear();
        }

        private async Task PlaceOrders(TradeState tradeState)
        {
            var currentPrice = tradeState.ExchangeData.CurrentPrice;

            var newOrders = tradeState.NewOrders.ToList();
            foreach (var order in newOrders)
            {
                if (!tradeState.CanPlaceOrder(order))
                    continue;

                //выставляемся выше, если цена выше желаемой
                if (order.Side == OrderSide.Sell && order.Price < currentPrice)
                    order.Price = currentPrice;

                var result = await client.PlaceOrder(tradeState.CurrencyPair.InstrumentId, order);
                if (!result)
                {
                    await notificationService.SendAlert(new AlertModel(AlertType.InternalError,
                        $"Не смогли выставить ордер:\n{order}"));
                    continue;
                }
                tradeState.ActiveOrders.Add(order);
                tradeState.NewOrders.Remove(order);
                tradeStateStore.Save(tradeState);
            }
        }

        private async Task<bool> CancelOrders(TradeState tradeState)
        {
            foreach (var order in tradeState.CancelOrders.OrderByDescending(order => order.Price))
            {
                if (order.Status != OrderStatus.Canceled)
                {
                    var result = await client.CancelOrder(tradeState.CurrencyPair.InstrumentId, order);
                    if (!result)
                        return false;
                }
                tradeState.CancelOrders.Remove(order);
                tradeStateStore.Save(tradeState);
            }
            return true;
        }
    }
}
