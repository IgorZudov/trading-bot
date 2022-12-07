using System;

namespace CryptoTrader.Utils.Throttler
{
    /// <summary>
    ///     Логика выполнения запросов без пауз в рамках окна сессии и уход в паузу при переполнении
    /// </summary>
    public class OverflowDeferLogic : IThrottleLogic
    {
        private struct TimeCounter
        {
            public readonly DateTime CreationTime;
            public int Value;

            public TimeCounter(DateTime creationTime, int value)
            {
                CreationTime = creationTime;
                Value = value;
            }
        }

        private readonly int requestPerSession;
        private readonly TimeSpan sessionTime;
        private TimeCounter counter;

        public OverflowDeferLogic(int rps, TimeSpan sessionTime)
        {
            requestPerSession = rps;
            this.sessionTime = sessionTime;
        }

        public TimeSpan GetDelay(DateTime now)
        {
            if (now - counter.CreationTime > sessionTime)
            {
                counter = new TimeCounter(now, 0);
            }

            if (requestPerSession == counter.Value)
            {
                var delay = CalculateDelay(now);
                counter = new TimeCounter(now + delay, 1);
                return delay;
            }

            counter.Value++;
            return TimeSpan.Zero;
        }

        private TimeSpan CalculateDelay(DateTime now)
        {
            var delayTime = sessionTime - (now - counter.CreationTime);
            var totalMs = delayTime.TotalMilliseconds;
            return totalMs > 0
                ? delayTime
                : TimeSpan.Zero;
        }
    }
}
