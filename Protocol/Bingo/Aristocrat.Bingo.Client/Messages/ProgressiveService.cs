﻿namespace Aristocrat.Bingo.Client.Messages
{
    using System;
    using System.Linq;
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
        private readonly IMessageHandlerFactory _messageHandlerFactory;

        public ProgressiveService(
            IMessageHandlerFactory messageHandlerFactory,
            IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient> endpointProvider)
            : base(endpointProvider)
        {
            _messageHandlerFactory =
                messageHandlerFactory ?? throw new ArgumentNullException(nameof(messageHandlerFactory));
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

            var size = result.ProgressiveLevel.Count;
            Logger.Debug($"RequestProgressiveInfoAsync response, size of response array={result.ProgressiveLevel.Count}");

            var progressiveLevels = new ProgressiveLevelInfo[size];
            var index = 0;
            foreach (var progressiveMapping in result.ProgressiveLevel)
            {
                Logger.Debug($"ProgressiveLevelInfo added, index={index}, level={progressiveMapping.ProgressiveLevel}, sequence={progressiveMapping.SequenceNumber}");

                progressiveLevels[index] = new ProgressiveLevelInfo(
                    progressiveMapping.ProgressiveLevel,
                    progressiveMapping.SequenceNumber);
                ++index;
            }

            Logger.Debug($"RequestProgressiveInfoAsync response, GameTitleId={result.GameTitleId}, Progressives={progressiveLevels.ToList()}");

            // TODO the response has no response code to check so how is failure detected and handled?
            // TODO this new class has to have response code and accepted to be an IResponse type.
            // TODO but there is no response code and accepted variables so just hard coding
            return new ProgressiveInfoResults(ResponseCode.Ok, true, result.GameTitleId, progressiveLevels);
        }
    }
}
