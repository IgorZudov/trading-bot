using System.Collections.Generic;
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
    public class StoplossModuleTest
    {
        private long id;
        private readonly CoreConfiguration configuration = new();

        [Test]
        public async Task Should_Collect_Buyed_Orders()
        {
            var state = CreateState();
            var module = CreateModule();
            state.ActiveOrders.Add(CreateOrder(OrderStatus.Filled, OrderSide.Buy));
            state.ActiveOrders.Add(CreateOrder(OrderStatus.Filled, OrderSide.Buy));
            await module.ProcessState(state);
            module.Buys.Count.Should().Be(2);
        }

        [Test]
        public async Task Should_Clear_Buys_If_Order_Selled()
        {
            var state = CreateState();
            var module = CreateModule();
            state.ActiveOrders.Add(CreateOrder(OrderStatus.Filled, OrderSide.Buy));
            state.ActiveOrders.Add(CreateOrder(OrderStatus.Filled, OrderSide.Buy));
            await module.ProcessState(state);
            state.ActiveOrders.Add(CreateOrder(OrderStatus.Filled, OrderSide.Sell));
            await module.ProcessState(state);
            module.Buys.Count.Should().Be(0);
        }

        [Test]
        public async Task Should_Create_Sell_With_Zero_Profit_If_Price_Below_Than_CurrentPrice()
        {
            var state = CreateState();
            state.MaxBuyDepth = 2;
            var module = CreateModule();
            var firstBuy = CreateOrder(OrderStatus.Filled, OrderSide.Buy);
            var secondBuy = CreateOrder(OrderStatus.Filled, OrderSide.Buy);

            state.ActiveOrders.AddRange(new List<Order>() { firstBuy, secondBuy });
            state.BuyedOrders.AddRange(new List<Order>() { firstBuy, secondBuy });

            state.ActiveOrders.Add(CreateOrder(OrderStatus.New, OrderSide.Sell));
            state.ExchangeData.CurrentPrice = 1 * (1 - 0.035m);
            await module.ProcessState(state);
            state.NewOrders.Count.Should().Be(1);
            state.CancelOrders.Count.Should().Be(1);
        }

        [Test]
        public async Task Should_Create_Sell_With_Market_Price_Profit_If_Price_Below_Than_CurrentPrice()
        {
            var state = CreateState();
            state.MaxBuyDepth = 2;
            var module = CreateModule();
            state.ActiveOrders.Add(CreateOrder(OrderStatus.Filled, OrderSide.Buy));
            state.ActiveOrders.Add(CreateOrder(OrderStatus.Filled, OrderSide.Buy));
            state.ActiveOrders.Add(CreateOrder(OrderStatus.New, OrderSide.Sell));
            state.ExchangeData.CurrentPrice = 1 * (1 - 0.055m);
            await module.ProcessState(state);
            state.NewOrders.Count.Should().Be(1);
            state.CancelOrders.Count.Should().Be(1);
        }

        private TradeState CreateState()
        {
            return new TradeState(configuration)
            {
                CurrencyPair = new CurrencyPair("BTC", 0, 8)
            };
        }

        private Order CreateOrder(OrderStatus status, OrderSide side, decimal price = 1, decimal amount = 1) =>
            new()
            {
                Status = status, Side = side, Id = id++.ToString(), Price = price, OriginalQuantity = amount
            };


        private StoplossModule CreateModule()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            return new StoplossModule(loggerFactory);
        }
    }
}
