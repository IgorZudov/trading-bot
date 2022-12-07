using System;
using AutoFixture;
using CryptoTrader.Core.Configuration;
using CryptoTrader.Core.Models;
using CryptoTrader.Persist.State;
using FluentAssertions;
using LiteDB;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CryptoTrader.IntegrationTests
{
    /// <summary>
    ///     Тесты на хранилище
    /// </summary>
    [TestFixture]
    class TradeStateStoreTests
    {
        private const string ConnectionString = "./store.db";

        private Fixture fixture;
        private CoreConfiguration configuration;
        private TradeStateStore store;
        private TradeState state;

        [SetUp]
        public void Setup()
        {
            configuration = CoreConfiguration.Default;

            fixture = new Fixture();
            fixture.Register(() => configuration);
            state = fixture.Build<TradeState>()
                .With(s => s.Id, "")
                .With(s => s.LastDealSetTime, DateTime.UtcNow)
                .With(s => s.LastSellTime, DateTime.UtcNow)
                .With(s => s.LastFirstBuyTime, DateTime.UtcNow)
                .Create();

            var db = new LiteDatabase(ConnectionString);
            store = new TradeStateStore(db, configuration);
            store.Clear();
        }

        [Test]
        public void Should_save_and_restore_state()
        {
            store.Save(state);
            var inserted = store.Get(state.Id);
            Assert(state, inserted);

            inserted.LimitDeposit = Randomizer.CreateRandomizer().NextDecimal();

            store.Save(inserted);
            var updated = store.Get(state.Id);
            Assert(inserted, updated);

            static void Assert(TradeState a, TradeState b) =>
                a.Should().BeEquivalentTo(b,
                    options => options.Excluding(tradeState => tradeState.ExchangeData)
                        .Excluding(tradeState => tradeState.TickId)
                        .Excluding(tradeState => tradeState.SignalInfo)
                        .Excluding(tradeState => tradeState.InstantFirstBuy)
                        .Excluding(tradeState => tradeState.IsActive)
                        .Excluding(tradeState => tradeState.CanBalance)
                        .Excluding(tradeState => tradeState.ExchangeWorkMode)
                        .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000))
                        .When(info => info.SelectedMemberPath.EndsWith("Time")));
        }

        [Test]
        public void Should_reset_store()
        {
            store.Save(state);
            store.Clear();
            var loaded = store.GetAll();
            loaded.Should().BeEmpty();
        }

        [Test]
        public void Should_return_empty_if_not_stored()
        {
            store.GetAll().Should().BeEmpty();
        }
    }
}
