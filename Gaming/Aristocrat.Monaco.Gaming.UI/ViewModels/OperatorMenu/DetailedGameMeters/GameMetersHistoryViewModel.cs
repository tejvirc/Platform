namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu.DetailedGameMeters
{
    public class GameMetersHistoryViewModel
    {
        public GameRoundMeterSnapshotViewModel BeforeGameStart { get; set; }
            = new GameRoundMeterSnapshotViewModel();

        public GameRoundMeterSnapshotViewModel AfterGameEnd { get; set; }
            = new GameRoundMeterSnapshotViewModel();

        public GameRoundMeterSnapshotViewModel BeforeNextGame { get; set; }
            = new GameRoundMeterSnapshotViewModel();
    }
}
