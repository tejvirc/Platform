namespace Aristocrat.Monaco.Bingo.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Services.Progressives;

    public class EnableByProgressiveResponseHandler : IProgressiveMessageHandler<ProgressiveUpdateResponse, EnableByProgressiveMessage>
    {
        private readonly IProgressiveUpdateHandler _updateHandler;

        public EnableByProgressiveResponseHandler(IProgressiveUpdateHandler updateHandler)
        {
            _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        }

        public async Task<ProgressiveUpdateResponse> Handle(EnableByProgressiveMessage enable, CancellationToken token)
        {
            var result = await _updateHandler.EnableByProgressive(enable, token);
            return new ProgressiveUpdateResponse(result ? ResponseCode.Ok : ResponseCode.Rejected);
        }
    }
}
