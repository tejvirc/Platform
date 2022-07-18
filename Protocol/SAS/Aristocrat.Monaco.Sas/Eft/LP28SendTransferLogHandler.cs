namespace Aristocrat.Monaco.Sas.Eft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.Eft;
    using Aristocrat.Sas.Client.Eft.Response;

    /// <summary>
    ///     The handler for LP28 Send EFT Transfer Logs
    /// </summary>
    public class LP28SendTransferLogHandler : ISasLongPollHandler<EftTransactionLogsResponse, LongPollData>
    {
        private readonly IEftHistoryLogProvider _eftHistoryLogProvider;

        /// <summary>
        ///     Constructs the handler
        /// </summary>
        /// <param name="eftHistoryLogProvider">The transaction history provider</param>
        public LP28SendTransferLogHandler(IEftHistoryLogProvider eftHistoryLogProvider)
        {
            _eftHistoryLogProvider = eftHistoryLogProvider ?? throw new ArgumentNullException(nameof(eftHistoryLogProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new() { LongPoll.EftSendTransferLogs };

        /// <summary>
        ///     Handle Command LP28 logic, return maximum 5 most recent eft transfer logs
        /// </summary>
        /// <param name="data"></param>
        /// <returns>EftTransactionLogsResponse</returns>
        public EftTransactionLogsResponse Handle(LongPollData data)
        {
            //confirmed that logs results will be in order (newest->first, oldest->last), and no need to do sorting here
            var eftHistoryLogs = _eftHistoryLogProvider.GetHistoryLogs().Take(SasConstants.EftHistoryLogsSize).ToArray();
            return new EftTransactionLogsResponse(eftHistoryLogs);
        }
    }
}