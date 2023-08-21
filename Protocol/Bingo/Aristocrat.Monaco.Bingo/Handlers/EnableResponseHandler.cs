namespace Aristocrat.Monaco.Bingo.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;

    public class EnableResponseHandler : IMessageHandler<EnableResponse, Enable>
    {
        private readonly IBingoDisableProvider _bingoDisableProvider;

        public EnableResponseHandler(IBingoDisableProvider bingoDisableProvider)
        {
            _bingoDisableProvider =
                bingoDisableProvider ?? throw new ArgumentNullException(nameof(bingoDisableProvider));
        }

        public Task<EnableResponse> Handle(Enable enable, CancellationToken token)
        {
            _bingoDisableProvider.Enable();

            return Task.FromResult(new EnableResponse(ResponseCode.Ok));
        }
    }
}