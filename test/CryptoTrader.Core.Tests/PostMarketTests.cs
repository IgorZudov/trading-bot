using System.Threading.Tasks;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Exchange;
using CryptoTrader.Core.Models.Orders;
using CryptoTrader.Core.TradeModules;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace CryptoTrader.Core.Tests
{
    [TestFixture]
    public class PostMarketTests
    {
        private const string Symbol = "NEBLBTC";
        private readonly CoreConfiguration configuration = new();

        private PostMarketModule syncModule;
        private TradeState state;

        [SetUp]
        public void Setup()
        {
            syncModule = new PostMarketModule();
            state = new TradeState(configuration, new CurrencyPair(Symbol, 0, 8))
            {
                ExchangeData = { CurrentPrice = 0.0023294m },
            };
        }

        [Test(Description =
            "3 активных целых бая и один сел. Должны добавить эквивалентный селл в новые, все активные в отмененные")]
        public async Task Should_cancel_buy_orders_and_add_sell_to_new()
        {
            //
            state.ActiveOrders.Add(new Order() { Id = "1", Side = OrderSide.Sell });
            state.ActiveOrders.Add(Buy());
            state.ActiveOrders.Add(Buy());
            state.ActiveOrders.Add(Buy());

            state.ExchangeWorkMode = ExchangeWorkMode.PostMarket;

            var otherModule = Substitute.For<ITradingModule>();
            syncModule.Next = otherModule;

            //act
            await syncModule.ProcessState(state);

            //assert
            await otherModule.DidNotReceive().ProcessState(state);

            state.ActiveOrders.Should().HaveCount(0);
            state.CancelOrders.Should().HaveCount(4);
            state.NewOrders.Should().HaveCount(1).And.Contain(c => c.Id == "1");
        }

        [Test(Description = "1 купленный бай, 2 целых бая. Ничего не делаем, просто ждем пока появится бай")]
        public async Task Should_skip_and_wait_process_filled_buy_order_and_add_sell_to_new()
        {
            state.ActiveOrders.Add(Buy(OrderStatus.Filled));
            state.ActiveOrders.Add(Buy());
            state.ActiveOrders.Add(Buy());

            state.ExchangeWorkMode = ExchangeWorkMode.PostMarket;

            var otherModule = Substitute.For<ITradingModule>();
            syncModule.Next = otherModule;

            //act
            await syncModule.ProcessState(state);

            //assert
            await otherModule.Received().ProcessState(state);
            state.ActiveOrders.Should().HaveCount(3).And.OnlyContain(o => o.Side == OrderSide.Buy);
            state.NewOrders.Should().HaveCount(0);
            state.CancelOrders.Should().HaveCount(0);
        }

        [Test(Description = "3 целых бая. Просто отменяем")]
        public async Task Should_cancel_buy_orders()
        {
            state.ActiveOrders.Add(Buy());
            state.ActiveOrders.Add(Buy());
            state.ActiveOrders.Add(Buy());

            state.ExchangeWorkMode = ExchangeWorkMode.PostMarket;

            var otherModule = Substitute.For<ITradingModule>();
            syncModule.Next = otherModule;

            //act
            await syncModule.ProcessState(state);

            //assert
            await otherModule.DidNotReceive().ProcessState(state);

            state.ActiveOrders.Should().HaveCount(0);
            state.NewOrders.Should().HaveCount(0);
            state.CancelOrders.Should().HaveCount(3);
        }

        [Test(Description = "Если нет активных ордеров, то не вызываем след модуль")]
        public async Task Should_dont_call_next_module_when_activeOrders_empty_and_postMarket()
        {
            state.ExchangeWorkMode = ExchangeWorkMode.PostMarket;

            var otherModule = Substitute.For<ITradingModule>();
            syncModule.Next = otherModule;

            //act
            await syncModule.ProcessState(state);

            //assert
            await otherModule.DidNotReceive().ProcessState(state);
            state.ActiveOrders.Should().HaveCount(0);
            state.NewOrders.Should().HaveCount(0);
            state.CancelOrders.Should().HaveCount(0);
        }

        private static Order Buy(OrderStatus status = OrderStatus.New) => new() { Side = OrderSide.Buy, Status = status };
    }
}
