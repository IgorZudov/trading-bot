using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using CryptoTrader.Core.TradeModules;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace CryptoTrader.Core.Tests
{
    [TestFixture]
    public class AdjusmentTest
    {
        private IList<Order> CreateOrders(OrderSide side, int count)
        {
            var orders = new List<Order>();
            for (int i = 0; i < count; i++)
            {
                orders.Add(new Order
                {
                    Id = i.ToString(),
                    Side = side
                });
            }

            return orders;
        }

        [Test]
        public async Task Should_Enter_Into_Deal_When_Reloading_Often()
        {
            var configuration = new CoreConfiguration();
            var state = CreateState(configuration);
            var module = CreateModule(configuration);
            await module.ProcessState(state);

            state.NewOrders.AddRange(CreateOrders(OrderSide.Buy, configuration.OrdersCount));
            state.CancelOrders.AddRange(CreateOrders(OrderSide.Buy, configuration.OrdersCount));

            await module.ProcessState(state);
            await module.ProcessState(state);
            await module.ProcessState(state);

            state.FirstStepDeviation.Should().Be(0);
        }

        [Test]
        public async Task Should_Increase_Profit_Percent_When_Sells_Often()
        {
            var configuration = new CoreConfiguration();
            var state = CreateState(configuration);
            var module = CreateModule(configuration);
            await module.ProcessState(state);

            var defaultTakeProfit = state.TakeProfit;
            state.ActiveOrders.Add(new Order() { Side = OrderSide.Sell });
            state.ActiveOrders.Where(order => order.Side == OrderSide.Sell)
                .OrderByDescending(order => order.Price)
                .First().Status = OrderStatus.Filled;
            await module.ProcessState(state);
            await module.ProcessState(state);
            await module.ProcessState(state);
            state.TakeProfit.Should().BeGreaterOrEqualTo(defaultTakeProfit);
        }

        private AdjustmentModule CreateModule(CoreConfiguration configuration)
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            return new AdjustmentModule(configuration, loggerFactory);
        }

        private TradeState CreateState(CoreConfiguration configuration)
        {
            var state = new TradeState(configuration)
            {
                ExchangeData = { CurrentPrice = 0.0023294m },
                CurrencyPair = new CurrencyPair("APPC", 2, 3)
            };
            return state;
        }
    }
}
