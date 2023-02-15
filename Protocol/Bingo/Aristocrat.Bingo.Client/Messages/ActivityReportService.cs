namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Protobuf.WellKnownTypes;
    using Polly;
    using ServerApiGateway;

    public sealed class ActivityReportService : BaseClientCommunicationService, IActivityReportService
    {
        private readonly IAsyncPolicy _policy;

        /// <summary>
        ///     Creates an instance of <see cref="ActivityReportService"/>
        /// </summary>
        /// <param name="endpointProvider">An instance of <see cref="IClientEndpointProvider{T}"/></param>
        public ActivityReportService(IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider)
            : base(endpointProvider)
        {
            _policy = CreatePolicy();
        }

        /// <inheritdoc />
        public async Task<ActivityResponseMessage> ReportActivity(ActivityReportMessage message, CancellationToken token = default)
        {
            var activityRequest = new ActivityRequest
            {
                MachineSerial = message.MachineSerial,
                ActivityTime = Timestamp.FromDateTime(message.ActivityTime)
            };

            var result = await Invoke(
                async (x, m, t) => await x.ReportActivityAsync(m, cancellationToken: t).ConfigureAwait(false),
                activityRequest,
                _policy,
                token).ConfigureAwait(false);
            return new ActivityResponseMessage(ResponseCode.Ok, result.ActivityResponseTime.ToDateTime());
        }
    }
}