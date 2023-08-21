namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System;

    /// <summary>
    ///     Data class to hold date and time
    /// </summary>
    public class LongPollDateTimeData : LongPollData
    {
        /// <summary>
        ///     Creates an instance of the LongPollDateTimeData class
        /// </summary>
        /// <param name="dateTime">Date time</param>
        public LongPollDateTimeData(DateTime dateTime)
        {
            DateAndTime = dateTime;
        }

        /// <summary>
        ///     Creates an instance of the LongPollDateTimeData class
        /// </summary>
        public LongPollDateTimeData()
        {
        }

        /// <summary>
        ///     Gets or sets the date time being requested
        /// </summary>
        public DateTime DateAndTime { get; set; }
    }

    /// <summary>
    ///     Data class to hold the response of a date time read
    /// </summary>
    public class LongPollDateTimeResponse : LongPollResponse
    {
        /// <summary>
        ///     Creates an instance of the LongPollDateTimeResponse class
        /// </summary>
        /// <param name="dateTime">The date time object</param>
        public LongPollDateTimeResponse(DateTime dateTime)
        {
            DateAndTime = dateTime;
        }

        /// <summary>
        ///     Gets or sets the date time being requested
        /// </summary>
        public DateTime DateAndTime { get; set; }
    }
}