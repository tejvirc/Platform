namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using log4net;

    /// <summary>
    ///     The handler for LP 7F Read Date Time
    /// </summary>
    public class LP7FReceiveDateAndTimeHandler : ISasLongPollHandler<LongPollReadSingleValueResponse<bool>, LongPollDateTimeData>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ITime _time;

        /// <summary>
        ///     Creates an instance of the LP7FReceiveDateAndTimeHandler class
        /// </summary>
        /// <param name="time">ITime interface object for sending datetime.</param>
        public LP7FReceiveDateAndTimeHandler(ITime time)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.ReceiveDateTime
        };

        /// <inheritdoc />
        public LongPollReadSingleValueResponse<bool> Handle(LongPollDateTimeData data)
        {
            var result = new LongPollReadSingleValueResponse<bool>(true);
            // set the time objects time. Log and error and return false if fails.
            if (!_time.Update(data.DateAndTime))
            {
                Logger.Error($"{data.DateAndTime} is out of range");
                result.Data = false;
            }

            return result;
        }
    }
}