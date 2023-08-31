namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using log4net;
    using ServerApiGateway;

    public sealed class RegistrationService :
        BaseClientCommunicationService<ClientApi.ClientApiClient>,
        IRegistrationService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IClient<ClientApi.ClientApiClient> _bingoClient;
        private readonly IBingoAuthorizationProvider _authorization;

        private bool _disposed;

        public RegistrationService(
            IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider,
            IClient<ClientApi.ClientApiClient> bingoClient,
            IBingoAuthorizationProvider authorization)
            : base(endpointProvider)
        {
            _bingoClient = bingoClient ?? throw new ArgumentNullException(nameof(bingoClient));
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));

            _bingoClient.Disconnected += OnClientDisconnected;
        }

        public Task<RegistrationResults> RegisterClient(
            RegistrationMessage message,
            CancellationToken token = default)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return RegisterClientInternal(message, token);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bingoClient.Disconnected -= OnClientDisconnected;
                _authorization.AuthorizationData = null;
            }

            _disposed = true;
        }

        private async Task<RegistrationResults> RegisterClientInternal(
            RegistrationMessage message,
            CancellationToken token)
        {
            var request = new RegistrationRequest
            {
                IsAuditCapable = true,
                MachineNumber = message.MachineNumber,
                MachineSerial = message.MachineSerial,
                PlatformVersion = message.PlatformVersion,
                MachineConnectionId = message.MachineConnectionId
            };

            var result = await Invoke(
                async (x, r, c) => await x.RequestRegisterAsync(r, cancellationToken: c),
                request,
                token).ConfigureAwait(false);
            if (result.ResultType is RegistrationResponse.Types.ResultType.Accepted &&
                !string.IsNullOrEmpty(result.AuthToken))
            {
                _authorization.AuthorizationData = new Metadata { { "Authorization", $"Bearer {result.AuthToken}" } };
            }

            Logger.Debug(
                $"Received a registration response with the status {result.ResultType} and with the message {result.Message}");
            return new RegistrationResults(result.ResultType.ToResponseCode(result.AuthToken), result.ServerVersion);
        }

        private void OnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            _authorization.AuthorizationData = null;
        }
    }
}