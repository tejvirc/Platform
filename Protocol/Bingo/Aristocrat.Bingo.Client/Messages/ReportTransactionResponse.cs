namespace Aristocrat.Bingo.Client.Messages
{
    public class ReportTransactionResponse : IResponse
    {
        public ReportTransactionResponse(ResponseCode responseCode, long transactionId)
        {
            ResponseCode = responseCode;
            TransactionId = transactionId;
        }

        public ResponseCode ResponseCode { get; }

        public long TransactionId { get; }
    }
}