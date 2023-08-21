namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.Contracts.Events;
    using Services.GamePlay;

    public class GameReplayCompletedConsumer : AsyncConsumes<GameReplayCompletedEvent>
    {
        private readonly IBingoReplayRecovery _recovery;
        private readonly IGameDiagnostics _gameDiagnostics;

        public GameReplayCompletedConsumer(IEventBus eventBus, ISharedConsumer context, IBingoReplayRecovery recovery, IGameDiagnostics gameDiagnostics)
            : base(eventBus, context)
        {
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
        }

        public override async Task Consume(GameReplayCompletedEvent theEvent, CancellationToken token)
        {
            if (!_gameDiagnostics.IsActive ||
                _gameDiagnostics.Context is not IDiagnosticContext<IGameHistoryLog> context)
            {
                return;
            }

            await _recovery.Replay(context.Arguments, true, token);
        }
    }
}