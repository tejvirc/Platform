namespace Aristocrat.Bingo.Client.Messages
{
    /// <summary>
    ///     The response for reporting a transaction to the server
    /// </summary>
    public class ReportTransactionResponse : IResponse
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReportTransactionResponse"/>
        /// </summary>
        /// <param name="responseCode">The response code for this message</param>
        /// <param name="transactionId">The transaction ID for this message</param>
        public ReportTransactionResponse(ResponseCode responseCode, long transactionId)
        {
            ResponseCode = responseCode;
            TransactionId = transactionId;
        }

        /// <summary>
        ///     Gets the response code
        /// </summary>
        public ResponseCode ResponseCode { get; }

        /// <summary>
        ///     Gets the transaction ID
        /// </summary>
        public long TransactionId { get; }
    }
}