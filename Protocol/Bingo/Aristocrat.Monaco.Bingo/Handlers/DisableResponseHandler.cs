namespace Aristocrat.Monaco.Bingo.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Aristocrat.Bingo.Client.Messages;
    using Localization.Properties;

    public class DisableResponseHandler : IMessageHandler<DisableResponse, Disable>
    {
        private readonly IBingoDisableProvider _bingoDisableProvider;

        public DisableResponseHandler(IBingoDisableProvider bingoDisableProvider)
        {
            _bingoDisableProvider =
                bingoDisableProvider ?? throw new ArgumentNullException(nameof(bingoDisableProvider));
        }

        public Task<DisableResponse> Handle(Disable disable, CancellationToken token)
        {
            var reason = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByBingoHost);
            var inputReason = disable.Reason?.Trim();
            if (!string.IsNullOrEmpty(inputReason))
            {
                reason += " - " + inputReason;
            }

            _bingoDisableProvider.Disable(reason);
            return Task.FromResult(new DisableResponse(ResponseCode.Ok));
        }
    }
}