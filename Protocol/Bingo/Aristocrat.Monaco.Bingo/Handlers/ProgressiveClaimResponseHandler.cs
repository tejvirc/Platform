namespace Aristocrat.Monaco.Bingo.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Services.Progressives;

    public class ProgressiveClaimResponseHandler : IMessageHandler<ProgressiveClaimResponse, ProgressiveClaimMessage>
    {
        private readonly IProgressiveClaimHandler _claimHandler;

        public ProgressiveClaimResponseHandler(IProgressiveClaimHandler claimHandler)
        {
            _claimHandler = claimHandler ?? throw new ArgumentNullException(nameof(claimHandler));
        }

        public async Task<ProgressiveClaimResponse> Handle(ProgressiveClaimMessage claim, CancellationToken token)
        {
            var result = await _claimHandler.ProcessProgressiveClaim(claim, token);
            return new ProgressiveClaimResponse(result ? ResponseCode.Ok : ResponseCode.Rejected);
        }
    }
}