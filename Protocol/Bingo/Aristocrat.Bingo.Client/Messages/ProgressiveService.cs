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
        IProgressiveService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public ProgressiveService(
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider)
            : base(endpointProvider)
        {
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

            var progressiveLevels = new List<ProgressiveLevelInfo>();
            foreach (var progressiveMapping in result.ProgressiveLevel)
            {
                Logger.Debug($"ProgressiveLevelInfo added, level={progressiveMapping.ProgressiveLevel}, sequence={progressiveMapping.SequenceNumber}");
                progressiveLevels.Add(new ProgressiveLevelInfo(progressiveMapping.ProgressiveLevel, progressiveMapping.SequenceNumber));
            }

            return new ProgressiveInfoResults(ResponseCode.Ok, true, result.GameTitleId, progressiveLevels);
        }
    }
}
