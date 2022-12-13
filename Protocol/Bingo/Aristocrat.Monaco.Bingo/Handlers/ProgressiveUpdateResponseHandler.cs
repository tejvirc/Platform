namespace Aristocrat.Monaco.Bingo.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Services.Progressives;

    public class ProgressiveUpdateResponseHandler : IMessageHandler<ProgressiveUpdateResponse, ProgressiveUpdateMessage>
    {
        private readonly IProgressiveUpdateHandler _updateHandler;

        public ProgressiveUpdateResponseHandler(IProgressiveUpdateHandler updateHandler)
        {
            _updateHandler = updateHandler ?? throw new ArgumentNullException(nameof(updateHandler));
        }

        public async Task<ProgressiveUpdateResponse> Handle(ProgressiveUpdateMessage update, CancellationToken token)
        {
            var result = await _updateHandler.ProcessProgressiveUpdate(update, token);
            return new ProgressiveUpdateResponse(result ? ResponseCode.Ok : ResponseCode.Rejected);
        }
    }
}
