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
    public class OverflowThrottlerTests
    {
        private const string Req = "req";
        private const int RequestPerSession = 60;

        private static RequestThrottler CreateThrottler(TimeSpan sessionTime) =>
            new RequestThrottler(sessionTime).AddOverflowDeferRequest(Req, RequestPerSession);

        [Test(Description = "Должны провести все запросы без задержек в пределах окна сессии")]
        public async Task Should_do_number_of_requests_in_session_time_without_overflow()
        {
            // arrange
            var sessionTime = TimeSpan.FromSeconds(3);
            var throttler = CreateThrottler(sessionTime);

            // act
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < RequestPerSession; i++)
            {
                await throttler.Throttle(Req);
            }
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange(0, 50); // ~50 ms overhead
        }

        [Test(Description =
            "Должны провести все запросы без задержек и потом уйти в ожидание на время в пределах окна сессии")]
        public async Task Should_do_number_of_requests_in_session_time_overflow()
        {
            // arrange
            var sessionTime = TimeSpan.FromSeconds(3);
            var throttler = CreateThrottler(sessionTime);

            // act
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < RequestPerSession + 1; i++)
            {
                await throttler.Throttle(Req);
            }
            var elapsed = sw.ElapsedMilliseconds;

            var sessionTotalMs =  (long) sessionTime.TotalMilliseconds;
            // assert
            elapsed.Should().BeInRange(sessionTotalMs, sessionTotalMs + 50); // ~50 ms overhead
        }

        [Test(Description = "Должны провести все запросы без задержек в пределах окна сессии учитывая паузы между ними")]
        public async Task Should_do_number_of_requests_in_session_time_without_overflow_with_pauses()
        {
            // arrange
            var pauseMs = 100;
            var iterCount = RequestPerSession * 2;

            var sessionTime = TimeSpan.FromSeconds(3);
            var throttler = CreateThrottler(sessionTime);

            // act
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterCount; i++)
            {
                await throttler.Throttle(Req);
                await Task.Delay(pauseMs);
            }
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange(iterCount * pauseMs, iterCount * pauseMs + 400); // ~400 ms overhead Task.Delay in loop
        }

        [Test(Description = @"Должны провести несколько запросов + подождать часть окна сессии
                            и после запросов уйти в ожидание на секунду")]
        public async Task Should_do_number_of_requests_partition_in_session_time_with_overflow()
        {
            // arrange
            var sessionTime = TimeSpan.FromSeconds(3);
            var throttler = CreateThrottler(sessionTime);

            // act
            await throttler.Throttle(Req);
            await Task.Delay(TimeSpan.FromSeconds(1));
            await throttler.Throttle(Req);
            await Task.Delay(TimeSpan.FromSeconds(1));

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < RequestPerSession; i++)
            {
                await throttler.Throttle(Req);
            }
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange(950, 1050); // ~100 ms overhead
        }

        [Test]
        [Explicit]
        public async Task Real_case()
        {
            // arrange
            var sessionTime = TimeSpan.FromMinutes(1);
            var throttler = CreateThrottler(sessionTime);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < RequestPerSession; i++)
            {
                await throttler.Throttle(Req);
            }
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange(0, 50); // ~50 ms overhead
        }

        [Test]
        [Explicit]
        public async Task Real_case_overflow()
        {
            // arrange
            var sessionTime = TimeSpan.FromMinutes(1);
            var throttler = CreateThrottler(sessionTime);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < RequestPerSession+1; i++)
            {
                await throttler.Throttle(Req);
            }
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange((long) sessionTime.TotalMilliseconds,
                (long) (sessionTime.TotalMilliseconds + 50)); // ~50 ms overhead
        }

        [Test]
        [Explicit]
        public async Task Real_case_overflow_parallel()
        {
            // arrange
            var sessionTime = TimeSpan.FromMinutes(1);
            var throttler = CreateThrottler(sessionTime);

            // act
            var tasks = Enumerable.Repeat(0, RequestPerSession + 1).Select(i => throttler.Throttle(Req));
            var sw = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange((long) sessionTime.TotalMilliseconds,
                (long) (sessionTime.TotalMilliseconds + 50)); // ~50 ms overhead
        }

        [Test]
        [Explicit]
        public async Task Real_case_parallel()
        {
            // arrange
            var sessionTime = TimeSpan.FromMinutes(1);
            var throttler = CreateThrottler(sessionTime);

            // act
            var tasks = Enumerable.Repeat(0, RequestPerSession).Select(i => throttler.Throttle(Req));
            var sw = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            var elapsed = sw.ElapsedMilliseconds;

            // assert
            elapsed.Should().BeInRange(0, 50); // ~50 ms overhead
        }
    }
}
