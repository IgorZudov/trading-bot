using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using Microsoft.Extensions.Logging;

namespace CryptoTrader.Core.TradeModules
{
    /// <summary>
    ///     Синхронизирует ордера при старте трейд системы
    /// </summary>
    public class OrdersSyncModule : IPreStartModule
    {
        private readonly IExchangeClient client;
        private readonly ILogger<OrdersSyncModule> logger;

        public OrdersSyncModule(IExchangeClient client, ILogger<OrdersSyncModule> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public async Task Invoke(TradeState tradeState)
        {
            if (CanSynchronize())
            {
                var ids = tradeState.ActiveOrders.ToDictionary(o => o.Id, o => o);
                var statusResult = await client.GetStatuses(tradeState.CurrencyPair!.InstrumentId, ids.Keys);
                if (!statusResult.Success)
                {
                    logger.LogError(statusResult.Error.Message);
                    return;
                }

                foreach (var (id, status) in statusResult.Value)
                {
                    if (ids.ContainsKey(id))
                    {
                        var order = ids[id];
                        switch (status)
                        {
                            case OrderStatus.Canceled:
                                tradeState.CancelOrder(id);
                                break;
                            case OrderStatus.Filled:
                                order.Status = OrderStatus.Filled;
                                order.ExecutedQuantity = order.OriginalQuantity;
                                break;
                        }
                    }
                }
            }

            bool CanSynchronize() => tradeState.CurrencyPair != null && tradeState.ActiveOrders.Count > 0;
        }
    }
}
