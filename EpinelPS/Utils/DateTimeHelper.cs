
namespace EpinelPS.Utils
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Get the specified time of the next day in the specified time zone
        /// </summary>
        /// <param name="timeZoneId">Time zone ID, such as "China Standard Time", "Eastern Standard Time", "UTC"</param>
        /// <param name="hour">Hours (0-23)</param>
        /// <param name="minute">Minutes (0-59)</param>
        /// <returns>The specified time of the next day</returns>
        public static DateTime GetNextDayAtTime(string timeZoneId = "", int hour = 0, int minute = 0)
        {
            // Get the current time of the target time zone and the current time zone
            (DateTime currentTime, TimeZoneInfo tz) = GetCurrentTimeWithZone(timeZoneId);

            DateTime nextDay = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hour, minute, 0).AddDays(1);

            return TimeZoneInfo.ConvertTimeToUtc(nextDay, tz);
        }

        /// <summary>
        /// Get the next specified day of the week + specified time in the specified time zone
        /// </summary>
        /// <param name="timeZoneId">Time zone ID, such as "China Standard Time"、"Eastern Standard Time"、"UTC"</param>
        /// <param name="targetDay">Target day of the week, for example DayOfWeek.Monday</param>
        /// <param name="hour">Hours (0-23)</param>
        /// <param name="minute">Minutes (0-59)</param>
        /// <returns>Next specified day of the week Specified time </returns>
        public static DateTime GetNextWeekdayAtTime(string timeZoneId, DayOfWeek targetDay, int hour, int minute = 0)
        {
            // Get the current time of the target time zone and the current time zone
            (DateTime currentTime, TimeZoneInfo tz) = GetCurrentTimeWithZone(timeZoneId);

            // Calculate the number of days until the target weekday
            int daysUntilTarget = ((int)targetDay - (int)currentTime.DayOfWeek + 7) % 7;
            if (daysUntilTarget == 0) daysUntilTarget = 7; // If today is the target day of the week, take next week

            DateTime nextWeekday = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hour, minute, 0).AddDays(daysUntilTarget);

            return TimeZoneInfo.ConvertTimeToUtc(nextWeekday, tz);
        }

        /// <summary>
        /// Get a specific day of next month in the specified time zone at a specified time
        /// </summary>
        /// <param name="timeZoneId">Time zone ID, such as "China Standard Time"、"Eastern Standard Time"、"UTC"</param>
        /// <param name="day">Target date (1-31, ensure that this date exists in the month)</param>
        /// <param name="hour">Hours (0-23)</param>
        /// <param name="minute">Minutes (0-59)</param>
        /// <returns>Specified date and time next month</returns>
        public static DateTime GetNextMonthDayAtTime(string timeZoneId, int day, int hour, int minute = 0)
        {
            // Get the current time of the target time zone and the current time zone
            (DateTime currentTime, TimeZoneInfo tz) = GetCurrentTimeWithZone(timeZoneId);

            // Calculate the year and month of next month
            int year = currentTime.Month == 12 ? currentTime.Year + 1 : currentTime.Year;
            int month = currentTime.Month == 12 ? 1 : currentTime.Month + 1;

            // Ensure the date is valid (avoid situations like 30th February)
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int targetDay = Math.Min(day, daysInMonth);

            // Construct target time
            DateTime target = new(year, month, targetDay, hour, minute, 0);

            return TimeZoneInfo.ConvertTimeToUtc(target, tz);
        }

        /// <summary>
        /// Get the current time and timezone object for a specified timezone
        /// </summary>
        /// <param name="timeZoneId">Time zone ID, such as "China Standard Time"、"Eastern Standard Time"、"UTC"</param>
        /// <returns>(currentTime, tz) tuple</returns>
        public static (DateTime currentTime, TimeZoneInfo tz) GetCurrentTimeWithZone(string timeZoneId)
        {

            // Get the target time zone
            TimeZoneInfo tz = TimeZoneInfo.Local;
            if (timeZoneId != null && timeZoneId != "")
                tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            return (currentTime, tz);
        }

    }
}

