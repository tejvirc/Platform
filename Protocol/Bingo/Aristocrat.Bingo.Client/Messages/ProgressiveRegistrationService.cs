namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using GamePlay;
    using Grpc.Core;
    using log4net;
    using Progressives;
    using ServerApiGateway;

    /// <summary>
    ///     Calls the bingo server to request progressive information and sets up the authentication
    ///     for the other progressive calls.
    /// </summary>
    public class ProgressiveRegistrationService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveRegistrationService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEnumerable<IClient> _clients;
        private readonly IProgressiveMessageHandlerFactory _messageHandlerFactory;
        private readonly IProgressiveAuthorizationProvider _authorization;
        private readonly IProgressiveLevelInfoProvider _progressiveLevelInfoProvider;
        private bool _disposed;

        public ProgressiveRegistrationService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IEnumerable<IClient> clients,
            IProgressiveMessageHandlerFactory messageHandlerFactory,
            IProgressiveAuthorizationProvider authorization,
            IProgressiveLevelInfoProvider progressiveLevelInfoProvider)
            : base(endpointProvider)
        {
            _clients = clients ?? throw new ArgumentNullException(nameof(clients));
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _messageHandlerFactory = messageHandlerFactory ?? throw new ArgumentNullException(nameof(messageHandlerFactory));
            _progressiveLevelInfoProvider = progressiveLevelInfoProvider ?? throw new ArgumentNullException(nameof(progressiveLevelInfoProvider));

            foreach (var client in _clients)
            {
                client.Disconnected += OnClientDisconnected;
            }
        }

        public async Task<RegistrationResults> RegisterClient(ProgressiveRegistrationMessage message, CancellationToken token)
        {
            Logger.Debug($"Progressive RegisterClient called, MachineSerial={message.MachineSerial}");

            Google.Protobuf.Collections.RepeatedField<ServerApiGateway.ProgressiveGame> games = new ();
            foreach (var progressiveGame in message.Games)
            {
                var game = new ServerApiGateway.ProgressiveGame
                {
                    GameTitleId = progressiveGame.GameTitleId,
                    Denomination = progressiveGame.Denomination,
                    MaxBet = progressiveGame.MaxBet
                };

                games.Add(game);

                Logger.Debug($"Registering progressive game GameTitleId={game.GameTitleId}, Denomination={game.Denomination}, MaxBet={game.MaxBet}");
            }

            var request = new ProgressiveInfoRequest
            {
                MachineSerial = message.MachineSerial,
                Games = { games }
            };

            var result = await Invoke(
                    async x => await x.RequestProgressiveInfoAsync(request, null, null, token));

            _authorization.AuthorizationData = new Metadata { { "Authorization", $"Bearer {result.AuthToken}" } };

            _progressiveLevelInfoProvider.ClearProgressiveLevelInfo();

            Logger.Debug("ProgressiveMapping returned from server:");
            foreach (var pm in result.ProgressiveLevels)
            {
                Logger.Debug($"ProgressiveMapping: level={pm.ProgressiveLevelId}, sequence={pm.SequenceNumber}, GameTitleId={pm.GameTitleId}, Denomination={pm.Denomination}");
            }

            var progressiveLevels = new List<ProgressiveLevelInfo>();
            foreach (var progressiveMapping in result.ProgressiveLevels)
            {
                Logger.Debug($"ProgressiveLevelInfo added, level={progressiveMapping.ProgressiveLevelId}, sequence={progressiveMapping.SequenceNumber}, GameTitleId={progressiveMapping.GameTitleId}, Denomination={progressiveMapping.Denomination}");

                progressiveLevels.Add(new ProgressiveLevelInfo(progressiveMapping.ProgressiveLevelId, progressiveMapping.SequenceNumber, progressiveMapping.GameTitleId, progressiveMapping.Denomination));

                _progressiveLevelInfoProvider.AddProgressiveLevelInfo(
                        progressiveMapping.ProgressiveLevelId,
                        progressiveMapping.SequenceNumber,
                        progressiveMapping.GameTitleId,
                        progressiveMapping.Denomination);
            }

            Logger.Debug("Meters To Report:");
            var metersToReport = new List<int>();
            foreach (var meter in result.MetersToReport)
            {
                metersToReport.Add(meter);
                Logger.Debug($"Meter{meter}");
            }

            var progressiveInfoMessage = new ProgressiveInfoMessage(
                ResponseCode.Ok,
                true,
                message.GameTitleId, // TODO result no longer has a game title id
                result.AuthToken,
                progressiveLevels,
                metersToReport);
            var handlerResult = await _messageHandlerFactory.Handle<ProgressiveInformationResponse, ProgressiveInfoMessage>(progressiveInfoMessage, token)
                .ConfigureAwait(false);

            return new RegistrationResults(handlerResult is null ? ResponseCode.Rejected : handlerResult.ResponseCode);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var client in _clients)
                {
                    client.Disconnected -= OnClientDisconnected;
                }

                _authorization.AuthorizationData = null;
            }

            _disposed = true;
        }

        private void OnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            _authorization.AuthorizationData = null;
        }
    }
}
