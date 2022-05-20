namespace Aristocrat.Monaco.Application.Contracts.Operations
{
    using System;

    /// <summary>
    ///     Contains the operating hours of an EGM for a particular day.
    /// </summary>
    public class OperatingHours
    {
        /// <summary>
        ///     Gets or sets the day of the week the time and state attributes apply to.
        /// </summary>
        public DayOfWeek Day { get; set; }

        /// <summary>
        ///     Gets or sets the offset from midnight, in milliseconds, at which the operating hours state interval begins
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the operating hours state should be enabled.
        /// </summary>
        public bool Enabled { get; set; }
    }
}