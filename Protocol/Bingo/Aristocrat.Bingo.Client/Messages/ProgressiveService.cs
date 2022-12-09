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
        private readonly IProgressiveAuthorizationProvider _authorization;
        private bool _isRegistered;
        private bool _disposed;
        private Thread _progressiveUpdateThread;
        private AsyncDuplexStreamingCall<Progressive, ProgressiveUpdate> _progressiveUpdateStream;

        public ProgressiveService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider,
            IProgressiveAuthorizationProvider authorization)
            : base(endpointProvider)
        {
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
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

        public async Task<ProgressiveUpdateResults> ProgressiveUpdates(ProgressiveUpdateRequestMessage message, CancellationToken token)
        {
            Logger.Debug("ProgressiveUpdates called");

            _progressiveUpdateStream = Invoke((x, c) => x.ProgressiveUpdates(null, null, c), token);

            if (!_isRegistered)
            {
                await _progressiveUpdateStream.RequestStream.WriteAsync(new Progressive { MachineSerial = message.MachineSerial });
                _isRegistered = true;
            }

            var responseStream = _progressiveUpdateStream.ResponseStream;
            var results = new ProgressiveUpdateResults();
            if (await responseStream.MoveNext(token).ConfigureAwait(false))
            {
                var data = new ProgressiveUpdateInfo(responseStream.Current.NewValue, responseStream.Current.ProgressiveLevel);
                Logger.Debug($"ProgressiveUpdateInfo, NewValue={data.NewValue}, progLevel={data.ProgressiveLevel}");
                results.UpdateInfo.Add(data);
            }

            _progressiveUpdateThread = new Thread(MonitorProgressiveUpdateStream)
            {
                Priority = ThreadPriority.Lowest
            };
            _progressiveUpdateThread.Start();

            // TODO just return bool if using thread
            return results;
        }

        private async void MonitorProgressiveUpdateStream()
        {
            Logger.Debug("MonitorProgressiveUpdateStream thread started");

            var responseStream = _progressiveUpdateStream.ResponseStream;
            var token = new CancellationToken();

            while (await responseStream.MoveNext(token).ConfigureAwait(false))
            {
                Logger.Debug($"Publishing ProgressiveUpdate, NewValue={responseStream.Current.NewValue}, ProgLevel={responseStream.Current.ProgressiveLevel}");
                var progressiveUpdate = new ProgressiveUpdate { ProgressiveLevel = responseStream.Current.ProgressiveLevel, NewValue = responseStream.Current.NewValue };
                // TODO how do I send this message to the system? event bus not seen here.
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
                _progressiveUpdateThread.Abort();
                _authorization.AuthorizationData = null;
            }

            _disposed = true;
        }
    }
}
