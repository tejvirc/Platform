namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.Contracts.Events;
    using Services.GamePlay;

    public class GameDiagnosticsStartedConsumer : AsyncConsumes<GameDiagnosticsStartedEvent>
    {
        private readonly IBingoReplayRecovery _recovery;

        public GameDiagnosticsStartedConsumer(IEventBus eventBus, ISharedConsumer context, IBingoReplayRecovery recovery)
            : base(eventBus, context)
        {
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
        }

        public override async Task Consume(GameDiagnosticsStartedEvent theEvent, CancellationToken token)
        {
            if (theEvent.Context is not IDiagnosticContext<IGameHistoryLog> context)
            {
                return;
            }

            await _recovery.Replay(context.Arguments, false, token);
        }
    }
}