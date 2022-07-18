namespace Aristocrat.Sas.Client.Eft.Response
{
    public class EftTransactionLogsResponse : LongPollResponse
    {
        /// <summary>
        ///     Initializes a new instance of the EftTransactionLogsResponse class.
        /// </summary>
        /// <param name="eftTransactionLogs">Maximum 5 transaction logs</param>
        public EftTransactionLogsResponse(IEftHistoryLogEntry[] eftTransactionLogs)
        {
            EftTransactionLogs = eftTransactionLogs;
        }

        /// <summary>
        ///     Maximum 5 most recent transfer logs
        /// </summary>
        public IEftHistoryLogEntry[] EftTransactionLogs { get; }
    }
}