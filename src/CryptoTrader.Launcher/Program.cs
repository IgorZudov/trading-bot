using System;
using System.Collections.Generic;
using System.Linq;
using Binance.Net;
using CryptoTrader.Binance;
using CryptoTrader.Core;
using CryptoTrader.Core.Buy;
using CryptoTrader.Core.Buy.BuyStrategy;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Helpers;
using CryptoTrader.Core.HypeAnalyzer;
using CryptoTrader.Core.HypeAnalyzer.Strategies;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.MarketWorkMode;
using CryptoTrader.Core.Models.Orders;
using CryptoTrader.Core.Queries;
using CryptoTrader.Core.Queries.Common;
using CryptoTrader.Core.Rebalancing;
using CryptoTrader.Core.Stores;
using CryptoTrader.Core.TradeModules;
using CryptoTrader.Core.TradeModules.Persist;
using CryptoTrader.Launcher.Services;
using CryptoTrader.Persist.Candles;
using CryptoTrader.Persist.CheckInstrument;
using CryptoTrader.Persist.HypeInstrument;
using CryptoTrader.Persist.State;
using CryptoTrader.Tinkoff;
using CryptoTrader.Utils.Throttler;
using DI.Trader.Telegram;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Scrutor;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace CryptoTrader.Launcher
{
    static class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                    builder.AddJsonFile("secrets.json", false, false))
                .UseSerilog((context, configuration) =>
                {
                    configuration.MinimumLevel.Verbose()
                        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                        .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day);
                })
                .ConfigureServices((ctx, services) =>
                {
                    services.AddOptions();
                    services.AddMemoryCache();
                    services.AddSingleton(sp =>
                    {
                        var config = sp.GetRequiredService<IConfiguration>();
                        return config.GetSection("ReBalancing").Get<ReBalancingOptions>();
                    }).AddSingleton(ctx.Configuration.GetSection("StretchingBuyOrders").Get<StretchingBuyOrdersOptions>());

                    services.AddSingleton<IMongoClient>(p =>
                        new MongoClient(ctx.Configuration.GetConnectionString("Mongo")))
                        .AddSingleton<BotClient>()
                        .AddSingleton<INotificationService, NotificationService>();

                    services
                        // hype
                        .Scan(s => s.AddType<HypeAnalyzer>().AsSelfWithInterfaces().WithSingletonLifetime())
                        .AddSingleton(p => CoreConfiguration.Default.HypeConfiguration)
                        .AddSingleton<MarketWorkModeProvider>()
                        .AddSingleton<ReBalancingService>()
                        .AddSingleton<BalanceCalculator>()

                        //queries
                        .AddTransient<IQueryHandler<GetTradeStatesResult>, GetStateQueryHandler>()

                        //strategies
                        .AddSingleton<IHypeStrategy, AmplitudeHypeStrategy>()
                        .AddSingleton<IReadOnlyCollection<IHypeStrategy>>(sp =>
                            sp.GetServices<IHypeStrategy>().ToList().AsReadOnly())

                        //modules
                        .AddTransient<ExchangeWorkModule>()
                        .AddTransient<PostMarketModule>()
                        .AddTransient<PersistStateModule>()
                        .AddTransient<ExchangeSyncModule>()
                        .AddTransient<HypeModule>()
                        .AddTransient<MiddleMartingale>()
                        .AddTransient<StoplossModule>()
                        .AddTransient<AdjustmentModule>()
                        .AddTransient<OrdersSyncModule>()

                        // TradeSystem
                        .AddSingleton(p => CoreConfiguration.Default)
                        .AddSingleton<ITradeStateStore, TradeStateStore>()
                        .AddSingleton<IHypeInstrumentStore, HypeInstrumentStore>()
                        .AddSingleton<ICheckInstrumentStore, CheckInstrumentStore>()
                        .AddSingleton<IBuyOrderStrategy, StairsBuyStrategy>()
                        .AddSingleton<ICandleStore, CandleStore>()
                        .AddSingleton<IExchangeDataProvider, ExchangeDataProvider>()
                        .AddSingleton<IQueryProcessor, QueryProcessor>()
                        .AddSingleton(p =>
                        {
                            var db = new LiteDatabase(ctx.Configuration.GetConnectionString("Db"));
                            db.Pragma("UTC_DATE", true);
                            return db;
                        })
                        .AddSingleton(p =>
                        {
                            //todo зарефакторить говнище

                            var coreConfig = p.GetRequiredService<CoreConfiguration>();
                            var stateStore = p.GetRequiredService<ITradeStateStore>();
                            var configuration = p.GetRequiredService<IConfiguration>();
                            var hypeInstrumentStore = p.GetRequiredService<IHypeInstrumentStore>();
                            var checkInstrumentStore = p.GetRequiredService<ICheckInstrumentStore>();

                            var resetState = configuration.GetValue<bool>("Reset");
                            if (resetState)
                                stateStore.Clear();

                            var resetInstruments = configuration.GetValue<bool>("ResetInstruments");
                            if (resetInstruments)
                            {
                                hypeInstrumentStore.Clear();
                                checkInstrumentStore.Clear();
                            }

                            var hypeInstruments = hypeInstrumentStore.Get();
                            var currentStates = stateStore.GetAll();

                            var tradeStates =
                                TradeStateFactory.CreateStates(coreConfig, currentStates, hypeInstruments);

                            var systems = tradeStates.Select(x =>
                            {
                                var system = new TradeSystem(x);

                                system.AddPreStartModule(Resolve<OrdersSyncModule>());

                                system
                                    .AddTradingModule(Resolve<PersistStateModule>())
                                    .AddTradingModule(Resolve<ExchangeSyncModule>())
                                    .AddTradingModule(Resolve<PostMarketModule>())
                                    .AddTradingModule(Resolve<ExchangeWorkModule>())
                                    .AddTradingModule(Resolve<HypeModule>())
                                    .AddTradingModule(Resolve<MiddleMartingale>())
                                    .AddTradingModule(Resolve<AdjustmentModule>());
                                return system;
                            });
                            return systems.ToList();
                            T Resolve<T>() => p.GetRequiredService<T>();
                        })

                        // exchange
                        .AddTypedOption<ApiSecrets>(ctx.Configuration.GetSection("ApiSecrets"))
                        .AddSingleton<IExchangeClient, BinanceClientAdapter>()
                        .AddSingleton<RequestThrottler>(_=>
                            new RequestThrottler()
                                .AddSplitBySessionRequest(ExchangeRequest.MarketRequest, 200)
                                .AddSplitBySessionRequest(ExchangeRequest.OrdersHistoryRequest, 120)
                                .AddOverflowDeferRequest(ExchangeRequest.OrdersRequest, 100)
                                .AddOverflowDeferRequest(ExchangeRequest.OrdersCancelRequest, 50)
                                .AddOverflowDeferRequest(ExchangeRequest.PlaceLimitOrderRequest, 50))


                        //workers
                        .AddHostedService<CheckInstrumentService>()
                        .AddHostedService<HypeService>()
                        .AddHostedService<TradeSystemService>()
                        .AddHostedService<LoadDataService>();
                })
                .Build()
                .Run();
        }

        private static IServiceCollection AddTypedOption<T>(this IServiceCollection serviceCollection,
            IConfiguration? configuration = null, Action<T>? configure = null) where T : class, new()
        {
            if (configuration != null)
                serviceCollection.Configure<T>(configuration);
            else if (configure != null)
                serviceCollection.Configure(configure);
            else
                serviceCollection.Configure<T>(delegate { });

            return serviceCollection.AddSingleton(p => p.GetRequiredService<IOptions<T>>().Value);
        }
    }
}
