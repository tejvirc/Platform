namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>
    ///     An interface to provide the functionalities related to the date and time
    ///     updating, formatting, and converting for display or other purposes.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Presently the parameter of type DateTime in the methods of
    ///         this interface must be in UTC. The OS clock will always be set
    ///         to UTC. This interface should be used to convert UTC times to local
    ///         times for display purpose.
    ///     </para>
    ///     <para>
    ///         It is supposed that the component implementing this interface will post
    ///         <see cref="TimeUpdatedEvent" /> when the date time is updated successfully.
    ///     </para>
    /// </remarks>
    public interface ITime
    {
        /// <summary>
        ///     Gets or sets the TimeZoneInformation.
        /// </summary>
        /// <value>
        ///     The current TimeZoneInformation.
        /// </value>
        TimeZoneInfo TimeZoneInformation { get; }

        /// <summary>
        ///     Gets or sets the time zone offset.
        /// </summary>
        /// <value>
        ///     The current time zone offset.
        /// </value>
        TimeSpan TimeZoneOffset { get; }

        /// <summary>
        ///     Gets the TimeSpan offset from Coordinated Universal Time (UTC).
        /// </summary>
        /// <remarks>
        ///     This takes into account the current timezone and the time zone offset.  Normally the time zone is set to UTC, but
        ///     in the event it isn't this will adjust for that.
        /// </remarks>
        TimeSpan TimeZoneBias { get; }

        /// <summary>
        ///     This method converts the DateTime into a location specific time with applied
        ///     time zone adjustments.
        /// </summary>
        /// <returns>A DateTime that has been converted to local time.</returns>
        /// <remarks>
        ///     In order to convert the supplied time to the wanted time, the used time
        ///     zone must be set. Otherwise, it will just return the time supplied through
        ///     the parameter.
        /// </remarks>
        /// <example>
        ///     <code>
        ///      // Get the time service.
        ///      ITime time = ...;
        /// 
        ///      // Convert the local time to the time for the currently used
        ///      // time zone.
        ///      string timeString = time.GetLocationTime(DateTime.UtcNow).ToString("G",
        ///                          CultureInfo.CurrentCulture);
        ///    </code>
        /// </example>
        DateTime GetLocationTime();

        /// <summary>
        ///     This method converts the DateTime into a location specific time with applied
        ///     time zone adjustments.
        /// </summary>
        /// <param name="time">The UTC DateTime to convert.</param>
        /// <returns>A DateTime that has been converted to local time.</returns>
        /// <remarks>
        ///     In order to convert the supplied time to the wanted time, the used time
        ///     zone must be set. Otherwise, it will just return the time supplied through
        ///     the parameter.
        /// </remarks>
        /// <example>
        ///     <code>
        ///      // Get the time service.
        ///      ITime time = ...;
        /// 
        ///      // Convert the local time to the time for the currently used
        ///      // time zone.
        ///      string timeString = time.GetLocationTime(DateTime.UtcNow).ToString("G",
        ///                          CultureInfo.CurrentCulture);
        ///    </code>
        /// </example>
        DateTime GetLocationTime(DateTime time);

        /// <summary>
        ///     This method converts the DateTime into a location specific time with applied
        ///     time zone adjustments.
        /// </summary>
        /// <param name="time">The UTC DateTime to convert.</param>
        /// <param name="timeZone">The TimeZoneInformation to use for conversion.</param>
        /// <returns>A DateTime that has been converted to local time.</returns>
        /// <remarks>
        ///     In order to convert the supplied time to the wanted time, the used time
        ///     zone must be set. Otherwise, it will just return the time supplied through
        ///     the parameter.
        /// </remarks>
        /// <example>
        ///     <code>
        ///      // Get the time service.
        ///      ITime time = ...;
        /// 
        ///      // Convert the local time to the time for the currently used
        ///      // time zone.
        ///      string timeString = time.GetLocationTime(DateTime.UtcNow).ToString("G",
        ///                          CultureInfo.CurrentCulture);
        ///    </code>
        /// </example>
        DateTime GetLocationTime(DateTime time, TimeZoneInfo timeZone);

        /// <summary>
        ///     Combines calls to GetLocationTime and FormatDateTimeString
        ///     to return a formatted string of the location specific time
        /// </summary>
        /// <returns></returns>
        string GetFormattedLocationTime();

        /// <summary>
        ///     Combines calls to GetLocationTime and FormatDateTimeString
        ///     to return a formatted string of the location specific time
        /// </summary>
        /// <param name="time">The UTC DateTime to convert.</param>
        /// <param name="format">The format of the datetime string</param>
        /// <returns></returns>
        string GetFormattedLocationTime(DateTime time, string format = ApplicationConstants.DefaultDateTimeFormat);

        /// <summary>
        ///     This method formats a DateTime object
        /// </summary>
        /// <param name="dateTime">The date to format</param>
        /// <param name="format">The format of the datetime string</param>
        /// <returns>A formatted date and time string</returns>
        /// <example>
        ///     <code>
        ///     // Get the time service.
        ///     ITime timeService = ...;
        ///     string timeString = timeService.FormatDateTimeString(DateTime.UtcNow);
        ///   </code>
        /// </example>
        string FormatDateTimeString(DateTime dateTime, string format = ApplicationConstants.DefaultDateTimeFormat);

        /// <summary>
        ///     This method takes a DateTimeOffset and updates the system time accordingly.
        /// </summary>
        /// <param name="time">The new time to update to.</param>
        /// <returns>True if updated; false otherwise.</returns>
        /// <exception cref="InvalidOperationException">
        ///     This exception will be thrown if it fails to set the system time for
        ///     unexpected reasons.
        /// </exception>
        /// <example>
        ///     <code>
        ///     DateTimeOffset time = new DateTimeOffset(DateTime.UtcNow, new TimeSpan(5, 0, 0));
        /// 
        ///     // Get the time service.
        ///     ITime timeService = ...;
        /// 
        ///     // Note that the <see cref="TimeUpdatedEvent" /> will be posted when the
        ///     // time is updated successfully.
        ///     if (timeService.Update(time))
        ///     {
        ///       // ...
        ///     }
        ///     else
        ///     {
        ///       // ...
        ///     }
        ///    </code>
        /// </example>
        bool Update(DateTimeOffset time);

        /// <summary>
        ///     This method update the datetimeformat of calling thread
        /// </summary>
        /// <example>
        ///     <code>
        ///     // Get the time service.
        ///     ITime timeService = ...;
        ///     string timeString = timeService.SetTheDateTimeForCurrentCulture();
        ///   </code>
        /// </example>
        void SetDateTimeForCurrentCulture();
    }
}
