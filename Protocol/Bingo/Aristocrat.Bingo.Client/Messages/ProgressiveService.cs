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
        private bool _enabled;
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

        public async Task<bool> RequestProgressiveInfo(ProgressiveInfoRequestMessage message, CancellationToken token)
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
            Logger.Debug($"AuthToken={result.AuthToken}");

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

            return handlerResult.ResponseCode == ResponseCode.Ok;
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
                // TODO what to do with this response
            }
            else if (progressiveMeta.Is(EnableByProgressive.Descriptor))
            {
                Logger.Debug("ReadProgressiveUpdate, ENABLE from progressive controller");
                _enabled = true;
                // TODO what to do with this response
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

            return true;
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
