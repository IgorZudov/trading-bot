using System.Threading.Tasks;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Exchange;
using CryptoTrader.Core.Models.Orders;
using CryptoTrader.Core.TradeModules;
using CryptoTrader.Core.TradeModules.Persist;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace CryptoTrader.Core.Tests
{
    [TestFixture]
    public class ExchangeSyncTests
    {
        private const string Symbol = "NEBLBTC";
        private readonly CoreConfiguration configuration = new();

        private IExchangeClient client;
        private ExchangeSyncModule syncModule;
        private TradeState state;

        [SetUp]
        public void Setup()
        {
            client = Substitute.For<IExchangeClient>();
            client.GetData(Symbol).Returns(new ExchangeData());

            var logger = Substitute.For<ILoggerFactory>();

            syncModule = new ExchangeSyncModule(client, logger,
                Substitute.For<INotificationService>(), Substitute.For<ITradeStateStore>());
            state = new TradeState(configuration)
            {
                ExchangeData = { CurrentPrice = 0.0023294m },
                CurrencyPair = new CurrencyPair(Symbol, 0, 8)
            };
        }


        [Test]
        public async Task Should_Not_Place_New_Orders_If_Cannot_Cancel()
        {
            client.CancelOrder(Symbol, new Order()).Returns(false);
            client.PlaceOrder(Symbol, Arg.Any<Order>()).Returns(true);

            //act
            state.AddNewBuyOrder(new Order());
            state.CancelOrders.Add(new Order());
            await syncModule.ProcessState(state);

            //assert
            await client.DidNotReceive().PlaceOrder(Symbol, Arg.Any<Order>());
        }


        [TestCase(OrderSide.Buy)]
        [TestCase(OrderSide.Sell)]
        public async Task Should_dont_clear_state_when_postMarket_and_cancel_failed(OrderSide side)
        {
            client.CancelOrder(state.CurrencyPair.InstrumentId, Arg.Is<Order>(o => o.Side == side)).Returns(false);

            state.ActiveOrders.Add(new Order() { Side = OrderSide.Sell });
            state.ActiveOrders.Add(new Order() { Side = OrderSide.Buy });
            state.ActiveOrders.Add(new Order() { Side = OrderSide.Buy });
            state.ActiveOrders.Add(new Order() { Side = OrderSide.Buy });

            state.ExchangeWorkMode = ExchangeWorkMode.PostMarket;

            var otherModule = Substitute.For<ITradingModule>();
            syncModule.Next = otherModule;

            //act
            await syncModule.ProcessState(state);

            //assert
            await otherModule.Received().ProcessState(state);
            state.ActiveOrders.Should().HaveCount(4);
            state.NewOrders.Should().HaveCount(0);
        }

        [Test]
        public async Task Should_dont_clear_state_when_postMarket_and_orders_in_deal()
        {
            client.CancelOrder(state.CurrencyPair.InstrumentId, Arg.Any<Order>()).Returns(true);

            state.ActiveOrders.Add(new Order() { Id = "1", Side = OrderSide.Sell });
            state.ActiveOrders.Add(new Order() { Side = OrderSide.Buy });
            state.ActiveOrders.Add(new Order() { Side = OrderSide.Buy, Status = OrderStatus.Filled });
            state.ActiveOrders.Add(new Order() { Side = OrderSide.Buy });

            state.ExchangeWorkMode = ExchangeWorkMode.PostMarket;

            var otherModule = Substitute.For<ITradingModule>();
            syncModule.Next = otherModule;

            //act
            await syncModule.ProcessState(state);

            //assert
            await otherModule.Received().ProcessState(state);
            state.ActiveOrders.Should().HaveCount(3);
            state.NewOrders.Should().HaveCount(0);
        }
    }
}
