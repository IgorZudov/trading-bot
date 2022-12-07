using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoTrader.Utils.Throttler
{
    public class RequestThrottler
    {
        private readonly Dictionary<string, IThrottleLogic> requests = new();
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        /// <summary>
        ///     Окно в котором считаеся кол-во запросов (default = 1 min)
        /// </summary>
        private readonly TimeSpan sessionTime;

        public RequestThrottler(TimeSpan? sessionTime = default) =>
            this.sessionTime = sessionTime ?? TimeSpan.FromMinutes(1);

        public async Task Throttle(string request)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                var requestModel = requests[request];
                var delay = requestModel.GetDelay(DateTime.UtcNow);
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public RequestThrottler AddSplitBySessionRequest(string request, int rps)
        {
            requests.TryAdd(request, new SplitBySessionLogic(rps, sessionTime));
            return this;
        }

        public RequestThrottler AddOverflowDeferRequest(string request, int rps)
        {
            requests.TryAdd(request, new OverflowDeferLogic(rps, sessionTime));
            return this;
        }
    }
}
