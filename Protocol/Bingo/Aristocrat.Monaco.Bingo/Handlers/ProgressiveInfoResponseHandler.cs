namespace Aristocrat.Monaco.Bingo.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Services.Progressives;
 
    public class ProgressiveInfoResponseHandler : IMessageHandler<ProgressiveInformationResponse, ProgressiveInfoMessage>
    {
        private readonly IProgressiveInfoHandler _infoHandler;

        public ProgressiveInfoResponseHandler(IProgressiveInfoHandler infoHandler)
        {
            _infoHandler = infoHandler ?? throw new ArgumentNullException(nameof(infoHandler));
        }

        public async Task<ProgressiveInformationResponse> Handle(ProgressiveInfoMessage info, CancellationToken token)
        {
            var result = await _infoHandler.ProcessProgressiveInfo(info, token);
            return new ProgressiveInformationResponse(result ? ResponseCode.Ok : ResponseCode.Rejected);
        }
    }
}
