namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;

    public class RequestProgressiveUpdateCommandHandler : ICommandHandler<ProgressiveUpdateRequestCommand>
    {
        private readonly IProgressiveUpdateService _progressiveService;

        public RequestProgressiveUpdateCommandHandler(IProgressiveUpdateService progressiveUpdateService)
        {
            _progressiveService = progressiveUpdateService ?? throw new ArgumentNullException(nameof(progressiveUpdateService));
        }

        /// <inheritdoc />
        public async Task Handle(ProgressiveUpdateRequestCommand command, CancellationToken token = default)
        {
            var message = new ProgressiveUpdateRequestMessage(command.MachineSerial);

            await _progressiveService.ProgressiveUpdates(message, token);
        }
    }
}
