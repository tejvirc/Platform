namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Protobuf.WellKnownTypes;
    using ServerApiGateway;

    /// <inheritdoc cref="Aristocrat.Bingo.Client.Messages.IReportEventService" />
    public class ReportEventService : BaseClientCommunicationService, IReportEventService
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReportEventService"/>
        /// </summary>
        /// <param name="endpointProvider">An instance of <see cref="IClientEndpointProvider{T}"/></param>
        public ReportEventService(IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
            : base(endpointProvider)
        {
        }

        /// <inheritdoc />
        public Task<ReportEventResponse> ReportEvent(ReportEventMessage message, CancellationToken token)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return ReportEventInternal(message, token);
        }

        private async Task<ReportEventResponse> ReportEventInternal(ReportEventMessage message, CancellationToken token)
        {
            var request = new EventReport
            {
                MachineSerial = message.MachineSerial,
                TimeStamp = message.TimeStamp.ToUniversalTime().ToTimestamp(),
                EventId = message.EventId,
                EventType = message.EventType
            };

            var result = await Invoke(
                    async (x, m, t) => await x.ReportEventAsync(m, null, null, t).ConfigureAwait(false),
                    request,
                    token)
                .ConfigureAwait(false);
            return new ReportEventResponse(result.Succeeded ? ResponseCode.Ok : ResponseCode.Rejected, result.EventId);
        }
    }
}