namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;

    public class RequestProgressiveInfoCommandHandler : ICommandHandler<ProgressiveInfoRequestCommand>
    {
        private readonly IProgressiveInfoService _progressiveInfoService;

        public RequestProgressiveInfoCommandHandler(IProgressiveInfoService progressiveService)
        {
            _progressiveInfoService = progressiveService ?? throw new ArgumentNullException(nameof(progressiveService));
        }

        /// <inheritdoc />
        public async Task Handle(ProgressiveInfoRequestCommand command, CancellationToken token = default)
        {
            var message = new ProgressiveInfoRequestMessage(
                command.MachineSerial,
                command.GameTitleId);

            _ = await _progressiveInfoService.RequestProgressiveInfo(message, token);
        }
    }
}
