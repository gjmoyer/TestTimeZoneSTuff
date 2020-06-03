using System;
using System.Collections.Generic;

namespace ConsoleApp8_TestTimeZoneSTuff
{
    using NodaTime;

    class Program
    {
        static DateTimeZone eastern;

        static void Main(string[] args)
        {
            eastern = DateTimeZoneProviders.Tzdb["America/New_York"];
            
            // -these timestamps should all be times greater than 5am today and less than 5am tomorrow

            // this is the various times of day that OOS messages will arrive
            var tslist = new Dictionary<DateTimeOffset, bool>()
            {
                { new DateTimeOffset(2020, 6, 2, 5, 45, 0, new TimeSpan(-4, 0, 0)), false },
                { new DateTimeOffset(2020, 6, 2, 10, 45, 0, new TimeSpan(-4, 0, 0)), false },
                { new DateTimeOffset(2020, 6, 2, 16, 0, 0, new TimeSpan(-4, 0, 0)), false },
                { new DateTimeOffset(2020, 6, 2, 23, 45, 0, new TimeSpan(-4, 0, 0)), false },
                { new DateTimeOffset(2020, 6, 3, 4, 45, 0, new TimeSpan(-4, 0, 0)), false },
                { new DateTimeOffset(2020, 6, 3, 4, 49, 59, new TimeSpan(-4, 0, 0)), false },
                { new DateTimeOffset(2020, 6, 3, 5, 45, 0, new TimeSpan(-4, 0, 0)), true },
                { new DateTimeOffset(2020, 6, 3, 23, 45, 0, new TimeSpan(-4, 0, 0)), true },
                { new DateTimeOffset(2020, 6, 3, 5, 00, 0, new TimeSpan(-4, 0, 0)), true },
                { new DateTimeOffset(2020, 6, 3, 15, 45, 0, new TimeSpan(-4, 0, 0)), true }
            };

            // this is simulating the current timestamp when messages will be processed
            var currentList = new List<DateTimeOffset>()
            {
                new DateTimeOffset(2020, 6, 2, 10, 00, 0, new TimeSpan(-4, 0, 0)),
                new DateTimeOffset(2020, 6, 2, 15, 49, 0, new TimeSpan(-4, 0, 0)),
                new DateTimeOffset(2020, 6, 3, 4, 49, 0, new TimeSpan(-4, 0, 0)),
                new DateTimeOffset(2020, 6, 2, 5, 49, 0, new TimeSpan(-4, 0, 0))
            };

            foreach (var currentTimestamp in currentList)
            {
                TestSet(tslist, currentTimestamp);
            }
        }

        static void TestSet(Dictionary<DateTimeOffset, bool> tslist, DateTimeOffset currentTimestamp)
        {
            foreach (var ts in tslist)
            {
                var result = CreateNewDailyList(ts.Key, currentTimestamp);
                Console.WriteLine("{0} {1} => new list = {2}  {3}", currentTimestamp, ts, result, result == ts.Value ? "" : "*");
            }
        }

        /// <summary>
        /// we must create a daily list based on the day starting at 5am eastern time
        /// </summary>
        /// <param name="listTimestamp">timestamp of list</param>
        /// <param name="currentTimestamp">simulate the time of day</param>
        /// <returns></returns>
        static bool CreateNewDailyList(DateTimeOffset listTimestamp, DateTimeOffset currentTimestamp)
        {
            var list = new ZonedDateTime(Instant.FromDateTimeOffset(listTimestamp), eastern);
            //var current = new ZonedDateTime(Instant.FromDateTimeUtc(DateTime.UtcNow), eastern);
            var current = new ZonedDateTime(Instant.FromDateTimeOffset(currentTimestamp), eastern);

            // my start of day will be 5am eastern time
            if (current.Hour < 5)
            {
                current = current.Date.PlusDays(-1).AtStartOfDayInZone(eastern).Plus(Duration.FromHours(5));
            }
            else
            {
                current = current.Date.AtStartOfDayInZone(eastern).Plus(Duration.FromHours(5));
            }

            return list.LocalDateTime < current.LocalDateTime || list.LocalDateTime >= current.Plus(Duration.FromDays(1)).LocalDateTime;
        }

    }
}
