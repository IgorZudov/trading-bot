using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeJam;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Notification;
using CryptoTrader.Core.Stores;
using Microsoft.Extensions.Hosting;

namespace CryptoTrader.Launcher.Services
{
    /// <summary>
    /// Сервис загрузки биржевых данных
    /// </summary>
    public class LoadDataService : BackgroundService
    {
        private readonly ICheckInstrumentStore store;
        private readonly IExchangeDataProvider provider;
        private readonly INotificationService notificationService;

        public LoadDataService(ICheckInstrumentStore store,
            IExchangeDataProvider provider, INotificationService notificationService)
        {
            this.store = store;
            this.provider = provider;
            this.notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var instruments = store.Get();
                    if (instruments == null || !instruments.Any())
                    {
                        await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                        continue;
                    }

                    var toProcess = new List<(bool isActual, CurrencyPair instrument)>(instruments.Count);

                    foreach (var instrument in instruments)
                    {
                        var actual = await provider.IsDataActual(instrument.InstrumentId);
                        toProcess.Add((actual, instrument));
                    }


                    foreach (var instrument in toProcess.OrderBy(x => x.isActual ? 0 : 1))
                    {
                        const int irrelevantLimit = 2;
                        TimeSpan? delay = null;
                        if(!instrument.isActual)
                            delay = TimeSpan.FromSeconds(irrelevantLimit);

                        await provider.Save(instrument.instrument.InstrumentId, delay);
                    }
                }
            }
            catch (Exception e) when(!(e is TaskCanceledException))
            {
                await notificationService.SendAlert(new AlertModel(AlertType.InternalError,
                    $"Сервис загрузки биржевых данных остановлен\n{e.ToDiagnosticString()}"));
            }
        }
    }
}
