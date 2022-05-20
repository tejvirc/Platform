namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;

    /// <summary>
    ///     Handles the bill denomination configuration
    /// </summary>
    public class LP08ConfigureBillDenominationsHandler : ISasLongPollHandler<LongPollResponse, LongPollBillDenominationsData>
    {
        private readonly ISasNoteAcceptorProvider _noteAcceptorProvider;

        /// <summary>
        ///     Creates a new instance of the LP08ConfigureBillDenominationsHandler
        /// </summary>
        /// <param name="noteAcceptorProvider">a reference to the ISasNoteAcceptorProvider class</param>
        public LP08ConfigureBillDenominationsHandler(ISasNoteAcceptorProvider noteAcceptorProvider)
        {
            _noteAcceptorProvider = noteAcceptorProvider ?? throw new ArgumentNullException(nameof(noteAcceptorProvider));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.ConfigureBillDenominations };

        /// <inheritdoc/>
        public LongPollResponse Handle(LongPollBillDenominationsData data)
        {
            Task.Run(
                () =>
                {
                    _noteAcceptorProvider.ConfigureBillDenominations(data.Denominations);
                    _noteAcceptorProvider.BillDisableAfterAccept = data.DisableAfterAccept;
                });
            return new LongPollResponse();
        }
    }
}