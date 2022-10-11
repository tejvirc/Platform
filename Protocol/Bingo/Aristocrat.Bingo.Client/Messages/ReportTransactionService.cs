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
            var request = new TransactionReport
            {
                MachineSerial = message.MachineSerial,
                TimeStamp = message.TimeStamp.ToUniversalTime().ToTimestamp(),
                Amount = message.Amount,
                GameSerial = message.GameSerial,
                GameTitleId = message.GameTitleId,
                TransactionId = message.TransactionId,
                PaytableId = message.PaytableId,
                Denomination = message.DenominationId,
                TransactionType = message.TransactionType,
                Barcode = message.Barcode ?? string.Empty
            };

            var result = await Invoke(
                    async (x, c) => await x.ReportTransactionAsync(request, null, null, c).ConfigureAwait(false),
                    token)
                .ConfigureAwait(false);

            return new ReportTransactionAck(result);
        }
    }
}