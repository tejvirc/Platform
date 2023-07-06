namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu.DetailedGameMeters
{
    using Aristocrat.Monaco.Gaming.Contracts;

    public class GameRoundMeterSnapshotViewModel
    {
        public GameRoundMeterSnapshot Snapshot { get; set; }
            = new GameRoundMeterSnapshot();

        public long TotalVouchersIn { get; set; }

        public long TotalVouchersOut { get; set; }

        public long WatOnTotalAmount { get; set; }

        public long WatOffTotalAmount { get; set; }

        public long TotalEgmPaidAmount { get; set; }

        public long TotalHandPaidAmount { get; set; }

        public long TotalPaidAmount { get; set; }

        public long TotalHardMeterOutAmount { get; set; }
    }
}
