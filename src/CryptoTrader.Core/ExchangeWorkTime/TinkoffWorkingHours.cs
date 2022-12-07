using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Exchange;

namespace CryptoTrader.Core.ExchangeWorkTime
{
    public static class TinkoffWorkingHours
    {
        private static DateTime currentTime;
        private static DateTime OpenTime => currentTime.Date.AddHours(12).AddMinutes(00);
        private static DateTime CloseTime => currentTime.Date.AddHours(23).AddMinutes(59);
        private static DateTime PostMarketStartTime => currentTime.Date.AddHours(23).AddMinutes(29);
        private static DateTime PreMarketStartTime => currentTime.Date.AddHours(10);


        public static ExchangeWorkMode GetWorkMode(DateTime dateInUtc)
        {
            var mskDate = dateInUtc.AddHours(3);
            currentTime = mskDate;
            if (mskDate.IsWeekends())
                return ExchangeWorkMode.DoesntWork;

            var date = mskDate.Date;
            if (Holidays.Any(x => x.Date == date))
                return ExchangeWorkMode.DoesntWork;

            if (mskDate >= PreMarketStartTime && mskDate <= OpenTime)
                return ExchangeWorkMode.PreMarket;

            if (mskDate > OpenTime && mskDate < PostMarketStartTime)
                return ExchangeWorkMode.FullyWorks;

            if (mskDate <= CloseTime && mskDate >= PostMarketStartTime)
                return ExchangeWorkMode.PostMarket;

            if (mskDate >= CloseTime && mskDate <= PreMarketStartTime)
                return ExchangeWorkMode.DoesntWork;

            return ExchangeWorkMode.DoesntWork;
        }

        private static readonly List<DateTime> Holidays = new()
        {
            new DateTime(2020, 1, 1),
            new DateTime(2020, 1, 20),
            new DateTime(2020, 2, 17),
            new DateTime(2020, 4, 10),
            new DateTime(2020, 5, 25),
            new DateTime(2020, 7, 3),
            new DateTime(2020, 7, 4),
            new DateTime(2020, 9, 7),
            new DateTime(2020, 11, 26),
            new DateTime(2020, 11, 27),
            new DateTime(2020, 12, 24),
            new DateTime(2020, 12, 25),
            new DateTime(2021, 1, 1),
            new DateTime(2021, 1, 18),
            new DateTime(2021, 2, 15),
            new DateTime(2021, 4, 2),
            new DateTime(2021, 5, 31),
            new DateTime(2021, 7, 5),
            new DateTime(2021, 9, 6),
            new DateTime(2021, 11, 25),
            new DateTime(2021, 11, 26),
            new DateTime(2021, 12, 24),
            new DateTime(2022, 1, 1)
        };

        private static bool IsWeekends(this DateTime date) =>
            date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
    }
}
