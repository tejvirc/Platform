namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Core;
    using log4net;
    using Progressives;
    using ServerApiGateway;

    public class ProgressiveService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IMessageHandlerFactory _messageHandlerFactory;
        private readonly IProgressiveAuthorizationProvider _authorization;
        private bool _isRegistered;
        private bool _disposed;
        private AsyncDuplexStreamingCall<Progressive, ProgressiveUpdate> _progressiveUpdateStream;

        public ProgressiveService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IMessageHandlerFactory messageHandlerFactory,
            IProgressiveAuthorizationProvider authorization)
            : base(endpointProvider)
        {
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _messageHandlerFactory = messageHandlerFactory ?? throw new ArgumentNullException(nameof(messageHandlerFactory));
        }

        public async Task<ProgressiveInfoResults> RequestProgressiveInfo(ProgressiveInfoRequestMessage message, CancellationToken token)
        {
            Logger.Debug($"RequestProgressiveInfo called, MachineSerial={message.MachineSerial}, GameTitleId={message.GameTitleId}");

            var request = new ProgressiveInfoRequest
            {
                MachineSerial = message.MachineSerial,
                GameTitleId = message.GameTitleId
            };

            var result = await Invoke(
                    async x => await x.RequestProgressiveInfoAsync(request, null, null, token));

            Logger.Debug($"RequestProgressiveInfoAsync response, size of response array={result.ProgressiveLevel.Count}");

            // TODO there is no Metadata in the progressive.proto so using core version
            _authorization.AuthorizationData = new Grpc.Core.Metadata { { "Authorization", $"Bearer {result.AuthToken}" } };

            var progressiveLevels = new List<ProgressiveLevelInfo>();
            foreach (var progressiveMapping in result.ProgressiveLevel)
            {
                Logger.Debug($"ProgressiveLevelInfo added, level={progressiveMapping.ProgressiveLevel}, sequence={progressiveMapping.SequenceNumber}");
                progressiveLevels.Add(new ProgressiveLevelInfo(progressiveMapping.ProgressiveLevel, progressiveMapping.SequenceNumber));
            }

            return new ProgressiveInfoResults(ResponseCode.Ok, true, result.GameTitleId, progressiveLevels);
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
            }

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var responseStream = _progressiveUpdateStream.ResponseStream;
                    while (await responseStream.MoveNext(token).ConfigureAwait(false) &&
                           await ReadProgressiveUpdate(responseStream.Current, token).ConfigureAwait(false))
                    {
                    }
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
            Logger.Debug($"ReadProgressiveUpdate, Progressive Level={_progressiveUpdateStream.ResponseStream.Current.ProgressiveLevel}, New Value={_progressiveUpdateStream.ResponseStream.Current.NewValue}, ");
            var progressiveUpdate = new ProgressiveUpdateMessage(ResponseCode.Ok, _progressiveUpdateStream.ResponseStream.Current.ProgressiveLevel, _progressiveUpdateStream.ResponseStream.Current.NewValue);
            var handlerResult = await _messageHandlerFactory.Handle<ProgressiveUpdateResponse, ProgressiveUpdateMessage>(progressiveUpdate, token)
                .ConfigureAwait(false);
            return handlerResult.ResponseCode == ResponseCode.Ok;
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
