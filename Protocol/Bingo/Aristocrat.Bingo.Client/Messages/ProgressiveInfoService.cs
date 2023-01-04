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

    /// <summary>
    ///     Calls the bingo server to request progressive information and sets up the authentication
    ///     for the other progressive calls.
    /// </summary>
    public class ProgressiveInfoService :
        BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>,
        IProgressiveInfoService,
        IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IMessageHandlerFactory _messageHandlerFactory;
        private readonly IProgressiveAuthorizationProvider _authorization;
        private bool _disposed;

        public ProgressiveInfoService(
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
