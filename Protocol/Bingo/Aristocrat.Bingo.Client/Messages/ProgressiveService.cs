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

    public class ProgressiveService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IProgressiveAuthorizationProvider _authorization;
        private bool _disposed;

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

            using var caller = Invoke((x, c) => x.ProgressiveUpdates(null, null, token), token);
            var responseStream = caller.ResponseStream;
            var results = new ProgressiveUpdateResults();
            if (await responseStream.MoveNext(token).ConfigureAwait(false))
            {
                var data = new ProgressiveUpdateInfo(responseStream.Current.NewValue, responseStream.Current.ProgressiveLevel);
                Logger.Debug($"ProgressiveUpdateInfo, NewValue={data.NewValue}, progLevel={data.ProgressiveLevel}");
                results.UpdateInfo.Add(data);
            }

            // TODO example code uses a while loop
            //while (await responseStream.MoveNext(token).ConfigureAwait(false))
            //{
            //    Logger.Debug($"SGL inside while loop, responseStream.Current={responseStream.Current}");

            //    var data = new ProgressiveUpdateInfo(responseStream.Current.NewValue, responseStream.Current.ProgressiveLevel);
            //    Logger.Debug($"ProgressiveUpdateInfo, NewValue={data.NewValue}, progLevel={data.ProgressiveLevel}");
            //    results.UpdateInfo.Add(data);
            //}

            return results;
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
