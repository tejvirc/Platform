namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Protobuf.WellKnownTypes;
    using ServerApiGateway;

    /// <inheritdoc cref="Aristocrat.Bingo.Client.Messages.IReportTransactionService" />
    public class ReportTransactionService : BaseClientCommunicationService<ClientApi.ClientApiClient>, IReportTransactionService
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReportTransactionService"/>
        /// </summary>
        /// <param name="endpointProvider">An instance of <see cref="IClientEndpointProvider{T}"/></param>
        public ReportTransactionService(
            IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
            : base(endpointProvider)
        {
        }

        /// <inheritdoc />
        public Task<ReportTransactionResponse> ReportTransaction(
            ReportTransactionMessage message,
            CancellationToken token)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return ReportTransactionInternal(message, token);
        }

        private async Task<ReportTransactionResponse> ReportTransactionInternal(
            ReportTransactionMessage message,
            CancellationToken token)
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