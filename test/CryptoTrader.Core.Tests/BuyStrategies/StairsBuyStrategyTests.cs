using CryptoTrader.Core.Buy.BuyStrategy;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Exchange;
using CryptoTrader.Core.Models.Orders;
using CryptoTrader.Utils;
using FluentAssertions;
using NUnit.Framework;

namespace CryptoTrader.Core.Tests.BuyStrategies
{
    //todo написать тесты на математику
    [TestFixture]
    public class StairsBuyStrategyTests
    {
        private readonly CoreConfiguration configuration = new()
        {
            LimitDeposit = 5000,
            OrdersCount = 3,
            PercentStep = 1,
            MartinPercent = 20,
            DepositOrder = 100
        };

        private StairsBuyStrategy CreateStrategy() => new(configuration, new StretchingBuyOrdersOptions());

        [Test]
        public void Should_Create_Buy_Orders()
        {
            var state = CreateState();
            state.CurrencyPair = new CurrencyPair("", 0, 8);
            state.BuyOrdersPrice = 100;
            state.ActiveOrders.Add(new Order
                { Status = OrderStatus.New, Side = OrderSide.Buy, Price = 100, OriginalQuantity = 10 });


            var strategy = CreateStrategy();
            strategy.CreateBuyOrders(state);

            state.NewOrders.Should().HaveCount(2);
            Assert.True(state.NewOrders.Exists(x => x.Price == 99 && x.OriginalQuantity == 12));
            Assert.True(state.NewOrders.Exists(x => x.Price == 98 && x.OriginalQuantity == 14));
        }


        [Test]
        public void Should_Create_One_Buy_Orders_When_First_Order_Greater_BuyOrderPrice()
        {
            var state = CreateState();
            state.CurrencyPair = new CurrencyPair("", 0, 8);
            state.BuyOrdersPrice = 100;
            state.ExchangeData.CurrentPrice = 220;
            state.ActiveOrders.Add(new Order
                { Status = OrderStatus.New, Side = OrderSide.Buy, Price = 200, OriginalQuantity = 10 });


            var strategy = CreateStrategy();
            strategy.CreateBuyOrders(state);

            state.NewOrders.Should().HaveCount(2);
            Assert.True(state.NewOrders.Exists(x => x.Price == 199m && x.OriginalQuantity == 12));
        }

        private TradeState CreateState()
        {
            SystemTime.ResetDateTime();
            var state = new TradeState(configuration)
            {
                ExchangeData = { CurrentPrice = 110m },
                ExchangeWorkMode = ExchangeWorkMode.FullyWorks,
                LimitDeposit = 1000,
                CurrencyPair = new CurrencyPair { InstrumentId = "123" }
            };
            state.CalculateMaxBuyDepth();
            return state;
        }
    }
}
