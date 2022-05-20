namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;

    /// <summary>
    ///     Definition of the TimeItem class.
    /// </summary>
    internal class TimeItem
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeItem" /> class.
        ///     Initializes a new instance of the TimeItem class
        /// </summary>
        /// <param name="display">The formatted time to display</param>
        /// <param name="time">A TimeSpan object</param>
        public TimeItem(string display, TimeSpan time)
        {
            Display = display;
            Time = time;
        }

        /// <summary>
        ///     Gets or sets the Display property
        /// </summary>
        public string Display { get; set; }

        /// <summary>
        ///     Gets or sets the Time property
        /// </summary>
        public TimeSpan Time { get; set; }

        /// <summary>
        ///     Determines if two objects have equivalent properties
        /// </summary>
        /// <param name="obj">The object to compare with this object</param>
        /// <returns>True if properties are equivalent; otherwise false</returns>
        public override bool Equals(object obj)
        {
            return obj is TimeItem item && Time == item.Time;
        }

        /// <summary>
        ///     Gets a hash code
        /// </summary>
        /// <returns>A hash code</returns>
        public override int GetHashCode()
        {
            return Time.GetHashCode();
        }
    }
}