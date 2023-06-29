namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu.DetailedGameMeters
{
    using System.Linq;
    using System.Reflection;
    using Contracts;

    /// <summary>
    ///     Builds a meter snapshot model with values pertaining to a specific game round.
    /// </summary>
    public class TransactionMeterFactory
    {

        private readonly IGameRoundMeterSnapshotProvider _meterSnapshotProvider;

        private static readonly PropertyInfo[] MeteredProps =
            typeof(GameRoundMeterSnapshot)
                .GetProperties()
                .Where(p => p.Name != nameof(GameRoundMeterSnapshot.PlayState) && p.Name != nameof(GameRoundMeterSnapshot.CurrentCredits))
                .ToArray();

        public TransactionMeterFactory(
            IGameRoundMeterSnapshotProvider meterSnapshotProvider)
        {
            _meterSnapshotProvider = meterSnapshotProvider;
        }

        public GameMetersHistoryViewModel Build(
            IGameHistoryLog previousGame,
            IGameHistoryLog currentGame,
            IGameHistoryLog nextGame)
        {

            var result = new GameMetersHistoryViewModel();

            if (currentGame == null)
            {
                return result;
            }

            FillBeforeGameStartTransactions(
                currentGame,
                result.BeforeGameStart,
                previousGame
            );

            FillAfterGameEndTransactions(
                currentGame,
                result.AfterGameEnd
            );

            FillBeforeNextGameStartTransactions(
                currentGame,
                result.BeforeNextGame,
                nextGame
            );

            return result;
        }

        private void FillBeforeGameStartTransactions(IGameHistoryLog gameHistoryLog,
            GameRoundMeterSnapshotViewModel result, IGameHistoryLog previousGame)
        {
            var beforeGameStartCurrentGame =
                gameHistoryLog?.MeterSnapshots
                .FirstOrDefault(s => s.PlayState == PlayState.PrimaryGameStarted);

            if (beforeGameStartCurrentGame == null)
            {
                return;
            }

            var resultingSnapshot = beforeGameStartCurrentGame;

            if (previousGame?.MeterSnapshots != null && previousGame.MeterSnapshots.Any())
            {
                var beforeNextGamePreviousGame = previousGame.MeterSnapshots.FirstOrDefault(
                    s => s.PlayState == PlayState.PresentationIdle || s.PlayState == PlayState.GameEnded);

                 resultingSnapshot = GetSnapshotDelta(beforeNextGamePreviousGame, beforeGameStartCurrentGame);
            }

            result.Snapshot = resultingSnapshot;
            FillMeters(result, resultingSnapshot);
        }

        private void FillAfterGameEndTransactions(IGameHistoryLog gameHistoryLog,
            GameRoundMeterSnapshotViewModel result)
        {
            var beforeGameStartCurrentGame =
                gameHistoryLog?.MeterSnapshots
                .FirstOrDefault(s => s.PlayState == PlayState.PrimaryGameStarted);

            var afterGameEnd = gameHistoryLog?.MeterSnapshots
                    .FirstOrDefault(s => s.PlayState == PlayState.Idle ||
                         s.PlayState == PlayState.GameEnded ||
                         s.PlayState == PlayState.PresentationIdle);

            if (beforeGameStartCurrentGame == null || afterGameEnd == null)
            {
                return;
            }

            var resultingSnapshot = GetSnapshotDelta(beforeGameStartCurrentGame, afterGameEnd);

            result.Snapshot = resultingSnapshot;
            FillMeters(result, resultingSnapshot);
        }

        private void FillBeforeNextGameStartTransactions(IGameHistoryLog gameHistoryLog,
            GameRoundMeterSnapshotViewModel result,
            IGameHistoryLog nextGameHistoryLog = null)
        {
            var afterGameEnd = gameHistoryLog?.MeterSnapshots
                .FirstOrDefault(s => s.PlayState == PlayState.Idle ||
                    s.PlayState == PlayState.GameEnded ||
                    s.PlayState == PlayState.PresentationIdle);

            if (afterGameEnd == null)
            {
                return;
            }

            var beforeNextGame = nextGameHistoryLog?.MeterSnapshots == null ||
                     !nextGameHistoryLog.MeterSnapshots.Any()
                ? _meterSnapshotProvider.GetSnapshot(PlayState.Idle)
                : nextGameHistoryLog.MeterSnapshots.FirstOrDefault(
                    s => s.PlayState == PlayState.PrimaryGameStarted
                );

            var resultingSnapshot = GetSnapshotDelta(afterGameEnd, beforeNextGame);

            result.Snapshot = resultingSnapshot;
            FillMeters(result, resultingSnapshot);
        }

        private static GameRoundMeterSnapshot GetSnapshotDelta(GameRoundMeterSnapshot olderSnapshot, GameRoundMeterSnapshot newerSnapshot)
        {
            var result = new GameRoundMeterSnapshot();
            result.CurrentCredits = newerSnapshot.CurrentCredits;

            foreach (var prop in MeteredProps)
            {
                var olderValue = (long)prop.GetValue(olderSnapshot, null);
                var newerValue = (long)prop.GetValue(newerSnapshot, null);

                prop.SetValue(result, newerValue - olderValue);
            }

            return result;
        }

        private static void FillMeters(GameRoundMeterSnapshotViewModel model, GameRoundMeterSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            model.TotalEgmPaidAmount =
                snapshot.EgmPaidGameWonAmount +
                snapshot.EgmPaidGameWinBonusAmount +
                snapshot.EgmPaidBonusAmount +
                snapshot.EgmPaidProgWonAmount;

            model.TotalHandPaidAmount =
                snapshot.HandPaidGameWonAmount +
                snapshot.HandPaidBonusAmount +
                snapshot.HandPaidGameWinBonusAmount +
                snapshot.HandPaidProgWonAmount;

            model.TotalPaidAmount =
                model.TotalEgmPaidAmount +
                model.TotalHandPaidAmount;

            model.TotalVouchersIn =
                snapshot.VoucherInCashableAmount +
                snapshot.VoucherInCashablePromoAmount +
                snapshot.VoucherInNonCashableAmount;

            model.TotalVouchersOut =
                snapshot.VoucherOutCashableAmount +
                snapshot.VoucherOutCashablePromoAmount +
                snapshot.VoucherOutNonCashableAmount;

            model.WatOnTotalAmount =
                snapshot.WatOnCashableAmount +
                snapshot.WatOnCashablePromoAmount +
                snapshot.WatOnNonCashableAmount;

            model.WatOffTotalAmount =
                snapshot.WatOffCashableAmount +
                snapshot.WatOffCashablePromoAmount +
                snapshot.WatOffNonCashableAmount;

            model.TotalHardMeterOutAmount =
                snapshot.HardMeterOutAmount;
        }
    }
}
