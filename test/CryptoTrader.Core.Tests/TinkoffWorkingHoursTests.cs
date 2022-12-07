using System;
using CryptoTrader.Core.ExchangeWorkTime;
using CryptoTrader.Core.Models.Exchange;
using FluentAssertions;
using NUnit.Framework;

namespace CryptoTrader.Core.Tests
{
    [TestFixture]
    public class TinkoffWorkingHoursTests
    {
        [Test]
        public void Should_Return_FullyWorks()
        {
            var time = new DateTime(2020, 8, 5, 13, 31, 0); //utc

            var state = TinkoffWorkingHours.GetWorkMode(time);

            state.Should().Be(ExchangeWorkMode.FullyWorks);
        }

        [Test]
        public void Should_Return_PostMarket()
        {
            var time = new DateTime(2020, 8, 5, 20, 39, 1); //utc

            var state = TinkoffWorkingHours.GetWorkMode(time);

            state.Should().Be(ExchangeWorkMode.PostMarket);
        }

        [Test]
        public void Should_Return_PreMarket()
        {
            var time = new DateTime(2020, 8, 5, 8, 0, 1); //utc

            var state = TinkoffWorkingHours.GetWorkMode(time);

            state.Should().Be(ExchangeWorkMode.PreMarket);
        }

        [Test]
        public void Should_Return_DoesNotWork()
        {
            var time = new DateTime(2020, 8, 5, 20, 59, 1); //utc

            var state = TinkoffWorkingHours.GetWorkMode(time);

            state.Should().Be(ExchangeWorkMode.DoesntWork);
        }
    }
}
