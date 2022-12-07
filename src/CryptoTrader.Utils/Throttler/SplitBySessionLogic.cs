using System;

namespace CryptoTrader.Utils.Throttler
{
    /// <summary>
    ///     Логика разпределения запросов с паузами в пределах окна
    /// </summary>
    public class SplitBySessionLogic : IThrottleLogic
    {
        private readonly int requestPerSession;
        private readonly TimeSpan sessionTime;

        private DateTime lastRequestTime;

        public SplitBySessionLogic(int rps, TimeSpan sessionTime)
        {
            requestPerSession = rps;
            this.sessionTime = sessionTime;
        }

        public TimeSpan GetDelay(DateTime now)
        {
            var delay = CalculateDelay(now);
            lastRequestTime = now + delay;
            return delay;
        }

        private TimeSpan CalculateDelay(DateTime now)
        {
            var minDelay = TimeSpan.FromMilliseconds(sessionTime.TotalMilliseconds / requestPerSession);
            var timespan = minDelay - (now - lastRequestTime);
            return timespan.TotalMilliseconds > 0
                ? timespan
                : TimeSpan.Zero;
        }
    }
}
