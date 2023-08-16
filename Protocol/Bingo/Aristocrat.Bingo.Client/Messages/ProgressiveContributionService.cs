namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using log4net;
    using Progressives;
    using ServerApiGateway;

    /// <summary>
    ///     Calls the bingo server to contribute to a progressive.
    /// </summary>
    public class ProgressiveContributionService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveContributionService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IProgressiveLevelInfoProvider _progressiveLevelInfoProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveContributionService" /> class.
        /// </summary>
        public ProgressiveContributionService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IProgressiveLevelInfoProvider progressiveLevelInfoProvider)
            : base(endpointProvider)
        {
            _progressiveLevelInfoProvider = progressiveLevelInfoProvider ??
                                            throw new ArgumentNullException(nameof(progressiveLevelInfoProvider));
        }

        /// <inheritdoc />
        public async Task<ProgressiveContributionResponse> Contribute(
            ProgressiveContributionRequestMessage message,
            CancellationToken token)
        {
            var request = new ReportCoin
            {
                CoinIn = message.CoinIn,
                MachineSerial = message.MachineSerial,
                GameTitleId = message.GameTitleId,
                InitialCoin = message.InitialCoin,
                OfflineCoin = message.OfflineCoin,
                Denomination = message.Denomination
            };

            Logger.Debug(
                $"ProgressiveContributionRequest, CoinIn={request.CoinIn}, MachineSerial={request.MachineSerial}, GameTitleId={request.GameTitleId}, Denomination={request.Denomination}");

            var result = await Invoke(async x => await x.ProgressiveContributionAsync(request, null, null, token));

            Logger.Debug($"ProgressiveCoinAck received, Accepted={result.Accepted}");

            return new ProgressiveContributionResponse(
                result.Accepted ? ResponseCode.Ok : ResponseCode.Rejected);
        }

        /// <inheritdoc />
        public Task<IEnumerable<(int, long)>> GetGamesUsingProgressive(int progressiveLevelId)
        {
            var games = _progressiveLevelInfoProvider.GetGamesUsingProgressive(progressiveLevelId);
            return Task.FromResult(games);
        }
    }
}
