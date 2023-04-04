namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu.DetailedGameMeters
{
    using Aristocrat.Monaco.Gaming.Contracts;

    public class GameRoundMeterSnapshotViewModelFactory
    {
        public GameRoundMeterSnapshotViewModel Build
            (GameRoundMeterSnapshot snapshot)
        {
            var result = new GameRoundMeterSnapshotViewModel
            {
                Snapshot = snapshot
            };

            if (snapshot == null)
            {
                return result;
            }

            result.TotalEgmPaidAmount =
                snapshot.EgmPaidGameWonAmount +
                snapshot.EgmPaidGameWinBonusAmount +
                snapshot.EgmPaidBonusAmount +
                snapshot.EgmPaidProgWonAmount;

            result.TotalHandPaidAmount =
                snapshot.HandPaidGameWonAmount +
                snapshot.HandPaidBonusAmount +
                snapshot.HandPaidGameWinBonusAmount +
                snapshot.HandPaidProgWonAmount;

            result.TotalPaidAmount =
                result.TotalEgmPaidAmount +
                result.TotalHandPaidAmount;

            result.TotalVouchersIn =
                snapshot.VoucherInCashableAmount +
                snapshot.VoucherInCashablePromoAmount +
                snapshot.VoucherInNonCashableAmount;

            result.TotalVouchersOut =
                snapshot.VoucherOutCashableAmount +
                snapshot.VoucherOutCashablePromoAmount +
                snapshot.VoucherOutNonCashableAmount;

            result.WatOnTotalAmount =
                snapshot.WatOnCashableAmount +
                snapshot.WatOnCashablePromoAmount +
                snapshot.WatOnNonCashableAmount;

            result.WatOffTotalAmount =
                snapshot.WatOffCashableAmount +
                snapshot.WatOffCashablePromoAmount +
                snapshot.WatOffNonCashableAmount;

            return result;
        }
    }
}