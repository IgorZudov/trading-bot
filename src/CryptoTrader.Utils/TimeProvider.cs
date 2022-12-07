using System;

namespace CryptoTrader.Utils
{
    public static class SystemTime
    {
        public static Func<DateTime> Now = () => DateTime.UtcNow;

        public static void SetDateTime(DateTime dateTimeNow)
        {
            Now = () => dateTimeNow;
        }

        public static void ResetDateTime()
        {
            Now = () => DateTime.UtcNow;
        }
    }
}
