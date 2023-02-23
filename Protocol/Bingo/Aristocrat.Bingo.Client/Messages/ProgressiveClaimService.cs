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
    ///     Calls the bingo server to claim a progressive.
    /// </summary>
    public class ProgressiveClaimService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveClaimService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IProgressiveLevelInfoProvider _progressiveLevelInfoProvider;

        public ProgressiveClaimService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IProgressiveLevelInfoProvider progressiveLevelInfoProvider)
            : base(endpointProvider)
        {
            _progressiveLevelInfoProvider = progressiveLevelInfoProvider ?? throw new ArgumentNullException(nameof(progressiveLevelInfoProvider));
        }

        public async Task<ProgressiveClaimResponse> ClaimProgressive(ProgressiveClaimRequestMessage message, CancellationToken token)
        {
            var serverProgressiveLevelIdId = _progressiveLevelInfoProvider.GetServerProgressiveLevelId(Convert.ToInt32(message.ProgressiveLevelId));
            if (serverProgressiveLevelIdId < 0)
            {
                throw new ArgumentException("Invalid progressive level id in ProgressiveClaimRequestMessage");
            }

            var request = new ClaimProgressiveWinRequest
            {
                MachineSerial = message.MachineSerial,
                ProgressiveLevelId = serverProgressiveLevelIdId,
                ProgressiveWinAmount = message.ProgressiveWinAmount
            };

            Logger.Debug($"ClaimProgressiveWinRequest, MachineSerial={request.MachineSerial}, ProgressiveLevelId={request.ProgressiveLevelId}, Amount={request.ProgressiveWinAmount}");

            var result = await Invoke(async x => await x.ClaimProgressiveWinAsync(request, null, null, token));

            Logger.Debug($"ProgressiveWinAck received, LevelId={result.ProgressiveLevelId}, WinAmount={result.ProgressiveWinAmount}, AwardId={result.ProgressiveAwardId}");

            // Win amount of zero indicates a negative acknowledgement
            if (result.ProgressiveWinAmount == 0)
            {
                return new ProgressiveClaimResponse(
                    ResponseCode.Rejected,
                    result.ProgressiveLevelId,
                    0L,
                    0);
            }

            return new ProgressiveClaimResponse(
                ResponseCode.Ok,
                result.ProgressiveLevelId,
                result.ProgressiveWinAmount,
                result.ProgressiveAwardId);
        }
    }
}
