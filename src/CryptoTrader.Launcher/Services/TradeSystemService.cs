using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeJam.Strings;
using CryptoTrader.Core;
using CryptoTrader.Core.HypeAnalyzer;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models.Notification;
using CryptoTrader.Core.Rebalancing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CryptoTrader.Launcher.Services
{
    public class TradeSystemService : BackgroundService
    {
        private List<TradeSystem> tradeSystems;
        private readonly ILogger<TradeSystemService> logger;
        private readonly ReBalancingService balancingService;
        private readonly IHypePosition hypePosition;
        private readonly INotificationService notificationService;
        private readonly TimeSpan delay;

        public TradeSystemService(List<TradeSystem> tradeSystems,
            IConfiguration configuration,
            ILogger<TradeSystemService> logger,
            ReBalancingService balancingService,
            IHypePosition hypePosition, INotificationService notificationService)
        {
            this.tradeSystems = tradeSystems;
            this.logger = logger;
            this.balancingService = balancingService;
            this.hypePosition = hypePosition;
            this.notificationService = notificationService;
            var sec = configuration.GetValue<int>("TradeSystemUpdateRate");
            delay = TimeSpan.FromSeconds(sec);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Trade system starting");

            await Start();
            await notificationService.SendInfo("Trade system started");
            logger.LogInformation("Trade system started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogTrace("Update");
                    logger.LogInformation("Current hype currencies count {@positions}",
                        hypePosition.Positions.Select(x => x.Pair.Name).Join(", "));


                    await UpdateSystems();
                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Trade system update error");
                    await notificationService.SendAlert(
                        new AlertModel(AlertType.InternalError, $"{e.Message}\n{e.StackTrace}"));
                    throw;
                }
            }
            await notificationService.SendInfo("Trade system stopped");
            logger.LogInformation("Trade system stopped");
        }

        private async Task Start()
        {
            foreach (var tradeSystem in tradeSystems)
            {
                try
                {
                    await tradeSystem.Start();
                }
                catch (Exception e)
                {
                    var id = tradeSystem.State.CurrencyPair?.Name ?? tradeSystem.State.Id;
                    logger.LogError(e, "Trade system start error {@nameOrId}", id);
                    throw;
                }
            }
        }

        private async Task UpdateSystems()
        {
            balancingService.ReBalance(tradeSystems.Select(x => x.State).ToList());

            tradeSystems = tradeSystems
                .OrderBy(x => x.State.CancelOrders.Count)
                .ThenByDescending(x => x.State.LimitDeposit)
                .ToList();

            var tickId = Guid.NewGuid().ToString("N");

            foreach (var tradeSystem in tradeSystems)
                await tradeSystem.Update(tickId);
        }
    }
}
