namespace Aristocrat.Monaco.Common
{
    using System;

    /// <summary>
    ///     Date time related extension methods
    /// </summary>
    public static class TimeExtensions
    {
        /// <summary>
        ///     Gets a string in the format of [-]hh:mm representing the offset between the provided datetime and the UTC datetime.
        /// </summary>
        /// <param name="this">The date and time to determine the offset for.</param>
        /// <returns>A string containing the offset.</returns>
        public static string GetFormattedOffset(this DateTime @this)
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(@this);

            return offset.ToString($"{(offset < TimeSpan.Zero ? "\\-" : "\\+")}hh\\:mm");
        }

        /// <summary>
        ///     Gets a string in the format of [-]hh:mm representing the offset between the provided datetime and the UTC datetime.
        /// </summary>
        /// <param name="this">The date and time to determine the offset for.</param>
        /// <returns>A string containing the offset.</returns>
        public static string GetFormattedOffset(this TimeSpan @this)
        {
            return @this.ToString($"{(@this < TimeSpan.Zero ? "\\-" : "\\+")}hh\\:mm");
        }

        /// <summary>
        /// Returns the elapsed time between now and the DateTime.  DateTime must by UTC.
        /// Protects against negative elapsed time due to negative time shifts of the clock.
        /// </summary>
        /// <param name="this">DateTime to compute elapsed time for</param>
        /// <param name="now">DateTime that was used to compute the Elapsed Time</param>
        /// <returns></returns>
        public static TimeSpan GetUtcElapsedTime(this DateTime @this, out DateTime now)
        {
            TimeSpan elapsed = TimeSpan.Zero;
            now = DateTime.UtcNow;

            if (now > @this)
            {
                elapsed += now - @this;
            }

            return elapsed;
        }
    }
}
