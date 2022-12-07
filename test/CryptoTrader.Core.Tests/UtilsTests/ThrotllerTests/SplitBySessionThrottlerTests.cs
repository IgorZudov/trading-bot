using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Utils.Throttler;
using FluentAssertions;
using NUnit.Framework;

namespace CryptoTrader.Core.Tests.UtilsTests.ThrotllerTests
{
    [TestFixture]
    public class SplitBySessionThrottlerTests
    {
        private const string Req = "req";
        private const int RequestPerSession = 60;

        private static RequestThrottler CreateThrottler(TimeSpan sessionTime) =>
            new RequestThrottler(sessionTime).AddSplitBySessionRequest(Req, RequestPerSession);


        [Test(Description = "Должны провести каждый запрос c задержкой в пределах окна сессии")]
        public async Task Should_do_number_of_requests_in_session_time()
        {
            // arrange
            var sessionTime = TimeSpan.FromSeconds(3);
            var throttler = CreateThrottler(sessionTime);

            var sw = Stopwatch.StartNew();
            // act +1 из за отсутствия ожидания первого запроса
            for (int i = 0; i < RequestPerSession + 1; i++)
            {
                await throttler.Throttle(Req);
            }
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange((long) sessionTime.TotalMilliseconds,
                (long) (sessionTime.TotalMilliseconds + 50)); // ~120 ms overhead
        }

        [Test(Description = "Должны провести каждый запрос c задержкой в пределах окна сессии (parallel)")]
        public async Task Should_do_number_of_requests_in_session_time_parallel()
        {
            // arrange
            var sessionTime = TimeSpan.FromSeconds(3);
            var throttler = CreateThrottler(sessionTime);

            // act
            var tasks = Enumerable.Repeat(0, RequestPerSession + 1).Select(i => throttler.Throttle(Req));
            var sw = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange((long) sessionTime.TotalMilliseconds,
                (long) (sessionTime.TotalMilliseconds + 50)); // ~120 ms overhead
        }

        [Test]
        [Explicit]
        public async Task Real_case()
        {
            // arrange
            var sessionTime = TimeSpan.FromMinutes(1);
            var throttler = CreateThrottler(sessionTime);

            // act +1 из за отсутствия ожидания первого запроса
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < RequestPerSession + 1; i++)
            {
                await throttler.Throttle(Req);
            }
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange((long) sessionTime.TotalMilliseconds - 100,
                (long) (sessionTime.TotalMilliseconds + 100)); // ~200 ms overhead
        }

        [Test]
        [Explicit]
        public async Task Real_case_split_parallel()
        {
            // arrange
            var sessionTime = TimeSpan.FromMinutes(1);
            var throttler = CreateThrottler(sessionTime);

            // act +1 из за отсутствия ожидания первого запроса
            var tasks = Enumerable.Repeat(0, RequestPerSession + 1).Select(i => throttler.Throttle(Req));
            var sw = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange((long) sessionTime.TotalMilliseconds,
                (long) (sessionTime.TotalMilliseconds + 100)); // ~150 ms overhead
        }
    }
}
