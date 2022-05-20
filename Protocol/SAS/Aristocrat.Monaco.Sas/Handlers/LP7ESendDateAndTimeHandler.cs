namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     The handler for LP 7E Send Date Time
    /// </summary>
    /// <inheritdoc />
    public class LP7ESendDateAndTimeHandler : ISasLongPollHandler<LongPollDateTimeResponse, LongPollData>
    {
        /// <summary>
        ///     store the ITime object for get time
        /// </summary>
        private readonly ITime _time;

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SetCurrentDateTime
        };

        /// <summary>
        ///     Creates an instance of the LP7ESendDateAndTimeHandler class
        /// </summary>
        /// <param name="time">ITime interface object for sending datetime.</param>
        public LP7ESendDateAndTimeHandler(ITime time)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        /// <inheritdoc />
        public LongPollDateTimeResponse Handle(LongPollData data)
        {
            return new LongPollDateTimeResponse(_time.GetLocationTime());
        }
    }
}