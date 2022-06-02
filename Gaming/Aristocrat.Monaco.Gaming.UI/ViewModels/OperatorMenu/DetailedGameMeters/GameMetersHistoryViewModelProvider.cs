namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu.DetailedGameMeters
{
    using System.Linq;
    using Aristocrat.Monaco.Gaming.Contracts;

    public class GameMetersHistoryViewModelProvider
    {
        private readonly GameRoundMeterSnapshotViewModelFactory _factory;
        private readonly GameRoundMeterSnapshotProvider _meterSnapshotProvider;

        public GameMetersHistoryViewModelProvider
            (GameRoundMeterSnapshotViewModelFactory factory,
            GameRoundMeterSnapshotProvider meterSnapshotProvider)
        {
            _factory = factory;
            _meterSnapshotProvider = meterSnapshotProvider;
        }

        public GameMetersHistoryViewModel Build(IGameHistoryLog currentGame,
            IGameHistoryLog nextGame)
        {
            var beforeGameStart =
                    currentGame.MeterSnapshots
                    .FirstOrDefault(s => s.PlayState == PlayState.PrimaryGameStarted);

            //There will only be a snapshot for an "Idle" game if the game round fails
            //and the machine is configured to persist failed game rounds.
            //For some games, the final modification to the game history log
            //may be conducted at the very final, "PresentationIdle" stage.
            var afterGameEnd = currentGame.MeterSnapshots
                    .FirstOrDefault(s => s.PlayState == PlayState.Idle ||
                                         s.PlayState == PlayState.GameEnded ||
                                         s.PlayState == PlayState.PresentationIdle);

            var beforeNextGame = nextGame?.MeterSnapshots == null ||
                                 !nextGame.MeterSnapshots.Any()
                ? _meterSnapshotProvider.GetSnapshot(PlayState.Idle)
                : nextGame.MeterSnapshots.FirstOrDefault(
                    s => s.PlayState == PlayState.PrimaryGameStarted
                );

            return new GameMetersHistoryViewModel
            {
                BeforeGameStart = _factory.Build(beforeGameStart),

                AfterGameEnd = _factory.Build(afterGameEnd),

                BeforeNextGame = _factory.Build(beforeNextGame)
            };
        }
    }
}
