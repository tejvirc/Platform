namespace Aristocrat.Monaco.Sas.Handlers
{
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;

    /// <summary>
    ///     Handles the Real Time Event Reporting command
    /// </summary>
    public class LP0ERealTimeEventReportingHandler : ISasLongPollHandler<LongPollResponse, EnableDisableData>, IRteStatusProvider
    {
        private const byte SasClient1 = 0;
        private const byte SasClient2 = 1;

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.EnableDisableRealTimeEventReporting };

        /// <inheritdoc/>
        public LongPollResponse Handle(EnableDisableData enableDisableData)
        {
            if (enableDisableData.Id == SasClient1)
            {
                Client1RteEnabled = enableDisableData.Enable;
            }
            else if (enableDisableData.Id == SasClient2)
            {
                Client2RteEnabled = enableDisableData.Enable;
            }

            return null;
        }

        /// <inheritdoc/>
        public bool Client1RteEnabled { get; private set; }

        /// <inheritdoc/>
        public bool Client2RteEnabled { get; private set; }
    }
}