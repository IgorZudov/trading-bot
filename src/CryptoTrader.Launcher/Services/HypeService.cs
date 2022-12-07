using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeJam.Strings;
using CryptoTrader.Core.HypeAnalyzer.Strategies;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models.Notification;
using CryptoTrader.Utils.Results;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CryptoTrader.Launcher.Services
{
    public class HypeService : BackgroundService
    {
        private readonly IReadOnlyCollection<IHypeStrategy> strategies;
        private readonly ILogger<HypeService> logger;
        private readonly INotificationService notificationService;

        public HypeService(IReadOnlyCollection<IHypeStrategy> strategies, ILogger<HypeService> logger,
            INotificationService notificationService)
        {
            this.strategies = strategies;
            this.logger = logger;
            this.notificationService = notificationService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = strategies.Select(s => RunStrategy(s, stoppingToken));
            return Task.WhenAll(tasks);
        }


        private async Task RunStrategy(IHypeStrategy strategy, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation($"Start: {strategy.Name}");
                    await strategy.Update();
                    strategy.GetSignals().OnSuccess(x =>
                    {
                        logger.LogInformation($"Hype: {strategy.Name}" +
                                              $" {x.SelectMany(x => $"{x.Pair.Name}").Join(", ")}");
                    });
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception e) when(!(e is TaskCanceledException))
                {
                    logger.LogError(e, $"Hype {strategy.Name} error");
                    await notificationService.SendAlert(new AlertModel(AlertType.InternalError,
                        $"{e.Message}\n{e.StackTrace}"));
                }
            }
        }
    }
}
