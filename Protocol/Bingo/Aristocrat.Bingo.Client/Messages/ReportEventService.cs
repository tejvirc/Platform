namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Protobuf.WellKnownTypes;
    using ServerApiGateway;

    public class ReportEventService :
        BaseClientCommunicationService<ClientApi.ClientApiClient>,
        IReportEventService
    {
        public ReportEventService(IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
            : base(endpointProvider)
        {
        }

        public async Task<ReportEventAck> ReportEvent(ReportEventMessage message, CancellationToken token)
        {
            var request = new EventReport()
            {
                MachineSerial = message.MachineSerial,
                TimeStamp = message.TimeStamp.ToUniversalTime().ToTimestamp(),
                EventId = message.EventId,
                EventType = message.EventType
            };

            var result = await Invoke(async x => await x.ReportEventAsync(request, null, null, token));

            return new ReportEventAck(result);
        }
    }
}