namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Protobuf.WellKnownTypes;
    using ServerApiGateway;

    public class ReportTransactionService :
        BaseClientCommunicationService,
        IReportTransactionService
    {
        public ReportTransactionService(
            IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
            : base(endpointProvider)
        {
        }

        public async Task<ReportTransactionAck> ReportTransaction(ReportTransactionMessage message, CancellationToken token)
        {
            var request = new TransactionReport()
            {
                MachineSerial = message.MachineSerial,
                TimeStamp = message.TimeStamp.ToUniversalTime().ToTimestamp(),
                Amount = message.Amount,
                GameSerial = message.GameSerial,
                GameTitleId = message.GameTitleId,
                TransactionId = message.TransactionId,
                PaytableId = message.PaytableId,
                DenominationId = message.DenominationId,
                TransactionType = message.TransactionType
            };

            var result = await Invoke(async x => await x.ReportTransactionAsync(request, null, null, token));

            return new ReportTransactionAck(result);
        }
    }
}