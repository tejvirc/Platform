namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Protobuf.WellKnownTypes;
    using ServerApiGateway;

    public class ReportEventService : BaseClientCommunicationService, IReportEventService
    {
        public ReportEventService(IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
            : base(endpointProvider)
        {
        }

        public async Task<ReportEventResponse> ReportEvent(ReportEventMessage message, CancellationToken token)
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