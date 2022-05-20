namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.Contracts.Events;
    using Services.GamePlay;

    public class GameConnectedConsumer : AsyncConsumes<GameConnectedEvent>
    {
        private readonly IBingoReplayRecovery _recovery;

        public GameConnectedConsumer(IEventBus eventBus, ISharedConsumer context, IBingoReplayRecovery recovery)
            : base(eventBus, context)
        {
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
        }

        public override async Task Consume(GameConnectedEvent theEvent, CancellationToken token)
        {
            if (theEvent.IsReplay)
            {
                return;
            }

            await _recovery.RecoverDisplay(token);
        }
    }
}