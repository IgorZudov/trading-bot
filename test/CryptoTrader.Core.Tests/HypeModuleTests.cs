using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.HypeAnalyzer;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;
using CryptoTrader.Core.Models.Orders;
using CryptoTrader.Core.TradeModules;
using CryptoTrader.Utils;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace CryptoTrader.Core.Tests
{
    [TestFixture]
    public class HypeModuleTests
    {
        readonly CoreConfiguration configuration = new();
        private readonly string id = Guid.NewGuid().ToString();
        public List<HypePositionSignal> CurrencyPairs { get; } = new()
        {
            new HypePositionSignal { Pair = new CurrencyPair("CNDBTC", 0, 8) },
            new HypePositionSignal { Pair = new CurrencyPair("WTCBTC", 0, 8) },
            new HypePositionSignal { Pair = new CurrencyPair("IOSTBTC", 0, 8) }
        };

        private TradeState CreateState()
        {
            SystemTime.ResetDateTime();
            var state = new TradeState(configuration)
            {
                Id = id,
                ExchangeData = { CurrentPrice = 0.0023294m },
                CurrencyPair = new CurrencyPair { InstrumentId = "123" }
            };
            return state;
        }

        private HypeModule CreateModule(ITradingModule otherModule)
        {
            var analyzer = Substitute.For<IHypePosition>();
            analyzer.GetPosition(id).Returns(CurrencyPairs[2]);
            var hypeModule = new HypeModule(analyzer)
            {
                Next = otherModule
            };
            return hypeModule;
        }

        [Test]
        public async Task Should_Call_Next_Module_If_Cannot_Cancel_Active_Orders_With_Partially_Buy()
        {
            var state = CreateState();
            state.CurrencyPair = new CurrencyPair("", 0, 8);
            state.ActiveOrders.Add(new Order { Status = OrderStatus.PartiallyFilled, Side = OrderSide.Buy });
            var otherModule = Substitute.For<ITradingModule>();
            var module = CreateModule(otherModule);
            await module.ProcessState(state);
            await otherModule.Received().ProcessState(state);
        }

        [Test]
        public async Task Should_Call_Next_Module_If_Cannot_Cancel_Active_Orders_with_Sell()
        {
            var state = CreateState();
            state.ActiveOrders.Add(new Order { Side = OrderSide.Sell });
            var otherModule = Substitute.For<ITradingModule>();
            var module = CreateModule(otherModule);
            await module.ProcessState(state);
            await otherModule.Received().ProcessState(state);
        }

        //todo: логика закрытия оредров при отсутствии хайпа переехала в ребалансер, новые тесты покроют эти кейсы


        // [Test]
        public async Task Should_Cancel_Active_Orders_And_Dont_Recieve_Next()
        {
            var state = CreateState();
            state.ActiveOrders.Add(new Order { Side = OrderSide.Buy, Status = OrderStatus.New });
            state.ActiveOrders.Add(new Order { Side = OrderSide.Buy, Status = OrderStatus.New });
            state.ActiveOrders.Add(new Order { Side = OrderSide.Buy, Status = OrderStatus.New });
            var otherModule = Substitute.For<ITradingModule>();
            var module = CreateModule(otherModule);
            await module.ProcessState(state);
            state.ActiveOrders.Count.Should().Be(0);
            state.CancelOrders.Count.Should().Be(3);
            await otherModule.DidNotReceive().ProcessState(state);
        }

        [Test]
        public async Task Should_Call_Next_Module_If_HypeCurrency_Equal_With_State_Currency()
        {
            var state = CreateState();
            state.CurrencyPair = CurrencyPairs[2].Pair;
            var otherModule = Substitute.For<ITradingModule>();
            var module = CreateModule(otherModule);
            await module.ProcessState(state);
            await otherModule.Received().ProcessState(state);
        }

        [Test]
        public async Task Should_Set_CurrencyPair_In_State_And_Dont_Recieve_Next()
        {
            var state = CreateState();
            var otherModule = Substitute.For<ITradingModule>();
            var module = CreateModule(otherModule);
            await module.ProcessState(state);
            state.CurrencyPair.InstrumentId.Should().Be(CurrencyPairs[2].Pair.InstrumentId);
            await otherModule.DidNotReceive().ProcessState(state);
        }
    }
}
