namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using log4net;
    using Progressives;
    using ServerApiGateway;

    /// <summary>
    ///     Calls the bingo server to award a progressive.
    /// </summary>
    public class ProgressiveAwardService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveAwardService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IProgressiveLevelInfoProvider _progressiveLevelInfoProvider;

        public ProgressiveAwardService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IProgressiveLevelInfoProvider progressiveLevelInfoProvider)
            : base(endpointProvider)
        {
            _progressiveLevelInfoProvider = progressiveLevelInfoProvider ?? throw new ArgumentNullException(nameof(progressiveLevelInfoProvider));
        }

        public async Task<ProgressiveAwardResponse> AwardProgressive(ProgressiveAwardRequestMessage message, CancellationToken token)
        {
            var serverProgressiveLevelIdId = _progressiveLevelInfoProvider.GetServerProgressiveLevelId(message.LevelId);
            if (serverProgressiveLevelIdId < 0)
            {
                Logger.Debug($"Invalid progressive level id {message.LevelId} in ProgressiveAwardRequestMessage");
                throw new ArgumentOutOfRangeException($"Invalid progressive level id {message.LevelId} in ProgressiveAwardRequestMessage");
            }

            var request = new ProgressiveAwardPaid
            {
                MachineSerial = message.MachineSerial,
                ProgressiveAwardId = message.ProgressiveAwardId,
                ProgressiveLevelId = serverProgressiveLevelIdId,
                Amount = message.Amount,
                Pending = message.Pending
            };

            Logger.Debug($"ProgressiveAwardPaid, MachineSerial={request.MachineSerial}, ProgressiveAwardId={request.ProgressiveAwardId}, LevelId={request.ProgressiveLevelId}, Amount={request.Amount}, Pending={request.Pending}");

            var result = await Invoke(async x => await x.AcknowledgeProgressiveWinAsync(request, null, null, token));

            Logger.Debug($"ProgressiveAwardPaidAck received, Success={result.Success}");

            return new ProgressiveAwardResponse(result.Success ? ResponseCode.Ok : ResponseCode.Rejected);
        }
    }
}
