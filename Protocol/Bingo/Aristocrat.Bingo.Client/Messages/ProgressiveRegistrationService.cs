namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
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
        private readonly IMessageHandlerFactory _messageHandlerFactory;
        private readonly IProgressiveAuthorizationProvider _authorization;
        private bool _disposed;

        public ProgressiveRegistrationService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IEnumerable<IClient> clients,
            IMessageHandlerFactory messageHandlerFactory,
            IProgressiveAuthorizationProvider authorization)
            : base(endpointProvider)
        {
            _clients = clients ?? throw new ArgumentNullException(nameof(clients));
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _messageHandlerFactory = messageHandlerFactory ?? throw new ArgumentNullException(nameof(messageHandlerFactory));

            foreach (var client in _clients)
            {
                client.Disconnected += OnClientDisconnected;
            }
        }

        public async Task<RegistrationResults> RegisterClient(ProgressiveRegistrationMessage message, CancellationToken token)
        {
            Logger.Debug($"RegisterClient called, MachineSerial={message.MachineSerial}, GameTitleId={message.GameTitleId}");

            var request = new ProgressiveInfoRequest
            {
                MachineSerial = message.MachineSerial,
                GameTitleId = message.GameTitleId
            };

            var result = await Invoke(
                    async x => await x.RequestProgressiveInfoAsync(request, null, null, token));

            _authorization.AuthorizationData = new Metadata { { "Authorization", $"Bearer {result.AuthToken}" } };

            var progressiveLevels = new List<ProgressiveLevelInfo>();
            foreach (var progressiveMapping in result.ProgressiveLevel)
            {
                Logger.Debug($"ProgressiveLevelInfo added, level={progressiveMapping.ProgressiveLevel}, sequence={progressiveMapping.SequenceNumber}");
                progressiveLevels.Add(new ProgressiveLevelInfo(progressiveMapping.ProgressiveLevel, progressiveMapping.SequenceNumber));
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
                result.GameTitleId,
                result.AuthToken,
                progressiveLevels,
                metersToReport);
            var handlerResult = await _messageHandlerFactory.Handle<ProgressiveInformationResponse, ProgressiveInfoMessage>(progressiveInfoMessage, token)
                .ConfigureAwait(false);

            return new RegistrationResults(handlerResult.ResponseCode);
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
            if (_clients.Any() != true)
            {
                _authorization.AuthorizationData = null;
            }
        }
    }
}
