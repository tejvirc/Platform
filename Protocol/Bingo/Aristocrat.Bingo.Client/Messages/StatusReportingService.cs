namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Polly;
    using ServerApiGateway;

    public sealed class StatusReportingService : BaseClientCommunicationService<ClientApi.ClientApiClient>, IStatusReportingService
    {
        private readonly IAsyncPolicy _retryPolicy;

        /// <summary>
        ///     Creates an instance of <see cref="StatusReportingService"/>
        /// </summary>
        /// <param name="endpointProvider">An instance of <see cref="IClientEndpointProvider{T}"/></param>
        public StatusReportingService(IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
            : base(endpointProvider)
        {
            _retryPolicy = CreatePolicy();
        }

        /// <inheritdoc />
        public async Task ReportStatus(StatusMessage message, CancellationToken token = default)
        {
            var statusResponse = new StatusReport
            {
                CashIn = message.CashInMeterValue,
                CashOut = message.CashOutMeterValue,
                CashPlayed = message.CashPlayedMeterValue,
                CashWon = message.CashWonMeterValue,
                StatusFlags = message.EgmStatusFlags,
                MachineSerial = message.MachineSerial
            };

            _ = await Invoke(
                async (x, m, t) => await x.ReportStatusAsync(m, cancellationToken: t).ConfigureAwait(false),
                statusResponse,
                _retryPolicy,
                token).ConfigureAwait(false);
        }
    }
}