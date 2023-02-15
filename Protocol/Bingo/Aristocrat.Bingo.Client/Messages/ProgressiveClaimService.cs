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
    ///     Calls the bingo server to claim a progressive.
    /// </summary>
    public class ProgressiveClaimService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveClaimService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEnumerable<IClient> _clients;
        private readonly IMessageHandlerFactory _messageHandlerFactory;
        private readonly IProgressiveAuthorizationProvider _authorization;
        private bool _disposed;

        public ProgressiveClaimService(
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

        public async Task<ProgressiveClaimResponse> ClaimProgressive(string machineSerial, long progressiveLevelId, long progressiveWinAmount, CancellationToken token)
        {
            // TODO progressive server wants the id's (10001, 10002, 10003) not the 0, 1, 2 in the message. Need to map this somewhere earlier.
            var mappedProgressives = new List<long>() { 10001L, 10002L, 10003L };

            var request = new ClaimProgressiveWinRequest
            {
                MachineSerial = machineSerial,
                ProgressiveLevelId = mappedProgressives[Convert.ToInt32(progressiveLevelId)],
                //ProgressiveLevelId = progressiveLevelId, // TODO progressive server wants the id's (10001, 10002, 10003) not the 0, 1, 2 in the message
                ProgressiveWinAmount = progressiveWinAmount,
            };

            Logger.Debug($"ClaimProgressiveWinRequest, MachineSerial={request.MachineSerial}, ProgressiveLevelId={request.ProgressiveLevelId}, Amount={request.ProgressiveWinAmount}");

            var result = await Invoke(async x => await x.ClaimProgressiveWinAsync(request, null, null, token));

            Logger.Debug($"ProgressiveWinAck received, LevelId={result.ProgressiveLevelId}, WinAmount={result.ProgressiveWinAmount}, AwardId={result.ProgressiveAwardId}");

            // Win amount of zero indicates a negative acknowledgement
            if (result.ProgressiveWinAmount == 0)
            {
                return new ProgressiveClaimResponse(ResponseCode.Rejected, result.ProgressiveLevelId, 0L, 0);
            }

            var claimMessage = new ProgressiveClaimMessage(
                result.ProgressiveLevelId,
                result.ProgressiveWinAmount,
                result.ProgressiveAwardId);

            var response = await _messageHandlerFactory
                .Handle<ProgressiveClaimResponse, ProgressiveClaimMessage>(claimMessage, token)
                .ConfigureAwait(false);

            return new ProgressiveClaimResponse(
                response.ResponseCode,
                response.ProgressiveLevelId,
                response.ProgressiveWinAmount,
                response.ProgressiveAwardId);
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
