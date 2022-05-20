namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Contracts;
    using Contracts.Progressives.Linked;
    using Kernel;
    using Progressives;

    public class PendingTriggerCommandHandler : ICommandHandler<PendingTrigger>
    {
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IProgressiveGameProvider _progressiveGame;
        private readonly IEventBus _bus;

        public PendingTriggerCommandHandler(
            IProgressiveGameProvider progressiveGame,
            IGameDiagnostics gameDiagnostics,
            IEventBus bus)
        {
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public void Handle(PendingTrigger command)
        {
            if (!_gameDiagnostics.IsActive)
            {
                _bus.Publish(new PendingLinkedProgressivesHitEvent(_progressiveGame.GetActiveLinkedProgressiveLevels()
                    .Select(l => l).Where(level => command.LevelIds.Contains(level.LevelId))));
            }
        }
    }
}