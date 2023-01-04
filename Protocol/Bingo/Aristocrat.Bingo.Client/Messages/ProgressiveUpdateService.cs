namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using log4net;
    using Progressives;
    using ServerApiGateway;

    public class ProgressiveUpdateService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveUpdateService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IMessageHandlerFactory _messageHandlerFactory;
        private readonly IProgressiveAuthorizationProvider _authorization;
        private bool _isRegistered;
        private bool _enabled;
        private bool _disposed;
        private AsyncDuplexStreamingCall<Progressive, ProgressiveUpdate> _progressiveUpdateStream;

        public ProgressiveUpdateService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IMessageHandlerFactory messageHandlerFactory,
            IProgressiveAuthorizationProvider authorization)
            : base(endpointProvider)
        {
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _messageHandlerFactory = messageHandlerFactory ?? throw new ArgumentNullException(nameof(messageHandlerFactory));
        }

        public async Task<bool> ProgressiveUpdates(ProgressiveUpdateRequestMessage message, CancellationToken token)
        {
            Logger.Debug("ProgressiveUpdates called");

            if (!_isRegistered)
            {
                // Open the stream
                _progressiveUpdateStream = Invoke((x, c) => x.ProgressiveUpdates(null, null, c), token);

                // Send the progressive message to start the progressive updates to the EGM
                await _progressiveUpdateStream.RequestStream.WriteAsync(new Progressive { MachineSerial = message.MachineSerial });
                _isRegistered = true;
                _enabled = true;
            }

            try
            {
                var responseStream = _progressiveUpdateStream.ResponseStream;
                while (await responseStream.MoveNext(token).ConfigureAwait(false) &&
                       await ReadProgressiveUpdate(responseStream.Current, token).ConfigureAwait(false))
                {
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Command service exited.  IsCancelled={token.IsCancellationRequested}", e);
                return false;
            }
        }
        private async Task<bool> ReadProgressiveUpdate(ProgressiveUpdate response, CancellationToken token)
        {
            if (response is null)
            {
                return false;
            }

            var progressiveMeta = response.ProgressiveMeta;
            if (progressiveMeta.Is(DisableByProgressive.Descriptor))
            {
                Logger.Debug("ReadProgressiveUpdate, DISABLE from progressive controller");
                _enabled = false;
                var disableByProgressive = new DisableByProgressiveMessage(ResponseCode.Ok);
                var handlerResult = await _messageHandlerFactory
                    .Handle<ProgressiveUpdateResponse, DisableByProgressiveMessage>(disableByProgressive, token)
                    .ConfigureAwait(false);
                return handlerResult.ResponseCode == ResponseCode.Ok;
            }
            else if (progressiveMeta.Is(EnableByProgressive.Descriptor))
            {
                Logger.Debug("ReadProgressiveUpdate, ENABLE from progressive controller");
                _enabled = true;
                var enableByProgressive = new EnableByProgressiveMessage(ResponseCode.Ok);
                var handlerResult = await _messageHandlerFactory
                    .Handle<ProgressiveUpdateResponse, EnableByProgressiveMessage>(enableByProgressive, token)
                    .ConfigureAwait(false);
                return handlerResult.ResponseCode == ResponseCode.Ok;
            }
            else
            {
                if (!_enabled)
                {
                    return true;
                }

                var update = progressiveMeta.Unpack<ProgressiveLevelUpdate>();
                Logger.Debug($"ReadProgressiveUpdate, Progressive Level={update.ProgressiveLevel}, New Value={update.NewValue}");
                var progressiveUpdate = new ProgressiveUpdateMessage(
                    ResponseCode.Ok,
                    update.ProgressiveLevel,
                    update.NewValue);
                var handlerResult = await _messageHandlerFactory
                    .Handle<ProgressiveUpdateResponse, ProgressiveUpdateMessage>(progressiveUpdate, token)
                    .ConfigureAwait(false);
                return handlerResult.ResponseCode == ResponseCode.Ok;
            }
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
                _authorization.AuthorizationData = null;
            }

            _disposed = true;
        }
    }
}
