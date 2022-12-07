using System;

namespace CryptoTrader.Utils.Throttler
{
    /// <summary>
    ///      Логика расчета времени тротлинга
    /// </summary>
    interface IThrottleLogic
    {
        TimeSpan GetDelay(DateTime now);
    }
}
