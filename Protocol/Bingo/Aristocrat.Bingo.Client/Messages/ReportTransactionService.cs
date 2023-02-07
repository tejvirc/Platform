namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Protobuf.WellKnownTypes;
    using ServerApiGateway;

    public class ReportTransactionService : BaseClientCommunicationService, IReportTransactionService
    {
        public ReportTransactionService(
            IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
            : base(endpointProvider)
        {
        }

        public async Task<ReportTransactionResponse> ReportTransaction(ReportTransactionMessage message, CancellationToken token)
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
                async (x, m, t) => await x.ReportTransactionAsync(m, null, null, t).ConfigureAwait(false),
                request,
                token).ConfigureAwait(false);

            return new ReportTransactionResponse(
                result.Succeeded ? ResponseCode.Ok : ResponseCode.Rejected,
                result.TransactionId);
        }
    }
}