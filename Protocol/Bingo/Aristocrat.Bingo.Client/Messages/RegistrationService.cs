﻿namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using log4net;
    using ServerApiGateway;

    public class RegistrationService :
        BaseClientCommunicationService,
        IRegistrationService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IClient _client;
        private readonly IAuthorizationProvider _authorization;

        private bool _disposed;

        public RegistrationService(
            IClientEndpointProvider<ClientApi.ClientApiClient> endpointProvider,
            IClient client,
            IAuthorizationProvider authorization)
            : base(endpointProvider)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _client.Disconnected += OnClientDisconnected;
        }

        public async Task<RegistrationResults> RegisterClient(RegistrationMessage message, CancellationToken token)
        {
            var request = new RegistrationRequest
            {
                IsAuditCapable = true,
                MachineNumber = message.MachineNumber,
                MachineSerial = message.MachineSerial,
                PlatformVersion = message.PlatformVersion
            };

            var result = await Invoke(async x => await x.RequestRegisterAsync(request, null, null, token));
            if (result.ResultType == RegistrationResponse.Types.ResultType.Accepted)
            {
                _authorization.AuthorizationData = new Metadata { { "Authorization", $"Bearer {result.AuthToken}" } };
            }
            
            Logger.Debug(
                $"Received a registration response with the status {result.ResultType} and with the message {result.Message}");
            return new RegistrationResults(result.ResultType.ToResponseCode(), result.ServerVersion);
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
                _client.Disconnected -= OnClientDisconnected;
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