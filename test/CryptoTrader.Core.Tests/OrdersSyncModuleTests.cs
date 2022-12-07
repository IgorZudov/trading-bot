using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using CryptoTrader.Core.Infrastructure;
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
    public class OrdersSyncModuleTests
    {
        private IExchangeClient exchangeClient;
        private OrdersSyncModule module;

        [SetUp]
        public void Setup()
        {
            exchangeClient = Substitute.For<IExchangeClient>();
            module = new OrdersSyncModule(exchangeClient, Substitute.For<ILogger<OrdersSyncModule>>());
        }

        [Test]
        [AutoData]
        public async Task Should_skip_sync_when_state_have_not_currency_pair(TradeState state)
        {
            state.CurrencyPair = null;
            await module.Invoke(state);
            await exchangeClient.DidNotReceiveWithAnyArgs().GetStatuses(default!, default!);
        }

        [Test]
        [AutoData]
        public async Task Should_update_statuses(TradeState state, List<Order> activeOrders)
        {
            state.ActiveOrders.AddRange(activeOrders);
            var ids = state.ActiveOrders.Select(o => o.Id).ToList();
            exchangeClient.GetStatuses(default!, default!).ReturnsForAnyArgs(new List<(string, OrderStatus)>
            {
                (ids[0], OrderStatus.Canceled),
                (ids[1], OrderStatus.Filled),
                (ids[2], OrderStatus.New)
            });
            await module.Invoke(state);

            state.CancelOrders.Should().ContainSingle(o => o.Id == ids[0]);
            state.ActiveOrders.Should().ContainSingle(o => o.Id == ids[1] && o.Status == OrderStatus.Filled);
        }
    }
}
