using System;
using System.Threading;
using System.Threading.Tasks;
using CodeJam;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models.Notification;
using CryptoTrader.Core.Stores;
using Microsoft.Extensions.Hosting;

namespace CryptoTrader.Launcher.Services
{
    /// <summary>
    /// Сервис загрузки списка инструментов
    /// </summary>
    public class CheckInstrumentService : BackgroundService
    {
        private readonly HypeConfiguration configuration;
        private readonly IExchangeClient client;
        private readonly ICheckInstrumentStore store;
        private readonly INotificationService notificationService;

        public CheckInstrumentService(HypeConfiguration configuration, IExchangeClient client,
            ICheckInstrumentStore store, INotificationService notificationService)
        {
            this.configuration = configuration;
            this.client = client;
            this.store = store;
            this.notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    //todo кэшировать инструменты к проверке нахлестом, чтобы не перебирать все 4к+ инструментов из неподходящего диапазона
                    var instruments = await client.GetExchangeInfo(configuration.BaseCurrency);
                    foreach (var instrument in instruments)
                    {
                        var data = await client.GetData(instrument.InstrumentId, false);
                        if(!data.Success)
                            continue;

                        var currentPrice = data.Value.CurrentPrice;
                        if (currentPrice >= configuration.MinPrice &&
                            currentPrice <= configuration.MaxPrice)
                        {
                            store.Save(instrument);
                        }
                    }
                    await Task.Delay(TimeSpan.FromDays(4), stoppingToken);
                }
            }
            catch (Exception e) when(!(e is TaskCanceledException))
            {
                await notificationService.SendAlert(new AlertModel(AlertType.InternalError,
                    $"Сервис загрузки инструментов остановлен\n{e.ToDiagnosticString()}"));
            }
        }
    }
}
