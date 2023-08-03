namespace Aristocrat.Monaco.Bingo.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Services.Progressives;

    public class DisableByProgressiveResponseHandler : IProgressiveMessageHandler<ProgressiveUpdateResponse, DisableByProgressiveMessage>
    {
        private readonly IProgressiveUpdateHandler _updateHandler;

        public DisableByProgressiveResponseHandler(IProgressiveUpdateHandler updateHandler)
        {
            _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        }

        public async Task<ProgressiveUpdateResponse> Handle(DisableByProgressiveMessage disable, CancellationToken token)
        {
            var result = await _updateHandler.DisableByProgressive(disable, token);
            return new ProgressiveUpdateResponse(result ? ResponseCode.Ok : ResponseCode.Rejected);
        }
    }
}
