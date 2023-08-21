namespace Aristocrat.Monaco.Sas
{
    using System;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.TransferOut;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Kernel;
    using Progressive;

    /// <summary>
    ///     Extension methods for handpay transactions
    /// </summary>
    public static class HandpayTransactionExtensions
    {
        private const uint NonLinkedProgressiveWinGroupId = 0;

        /// <summary>
        ///     Creates the handpay response data from the provided handpay transaction
        /// </summary>
        /// <param name="this">The transaction to covert</param>
        /// <param name="propertiesManager">An instance of IPropertiesManager</param>
        /// <param name="bank">An instance of IBank</param>
        /// <param name="gameHistory">An instance of IGameHistory</param>
        /// <param name="progressiveWinDetailsProvider">An instance of IProgressiveWinDetailsProvider</param>
        /// <returns>The long response data for the provided handpay</returns>
        public static LongPollHandpayDataResponse ToHandpayDataResponse(
            this HandpayTransaction @this,
            IPropertiesManager propertiesManager,
            IBank bank,
            IGameHistory gameHistory,
            IProgressiveWinDetailsProvider progressiveWinDetailsProvider)
        {
            var winAmount = @this.WinAmount();
            var (level, progressiveGroup) = GetProgressiveHandpayInfo(@this, gameHistory, progressiveWinDetailsProvider);

            return new LongPollHandpayDataResponse
            {
                Amount = winAmount,
                Level = level,
                ResetId = @this.GetResetId(propertiesManager, bank),
                PartialPayAmount = GetPartialPayAmount(@this, gameHistory),
                ProgressiveGroup = progressiveGroup,
                SessionGamePayAmount = winAmount,
                SessionGameWinAmount = winAmount,
                TransactionId = @this.TransactionId
            };
        }

        /// <summary>
        ///     Gets the reset id for SAS for a given handpay transaction
        /// </summary>
        /// <param name="this">The handpay transaction to get the reset ID for</param>
        /// <param name="propertiesManager">An instance of IPropertiesManager</param>
        /// <param name="bank">An instance of IBank</param>
        /// <returns>The reset ID for SAS for this handpay</returns>
        public static ResetId GetResetId(
            this HandpayTransaction @this,
            IPropertiesManager propertiesManager,
            IBank bank)
        {
            var resetMethod = propertiesManager.GetValue(
                AccountingConstants.LargeWinHandpayResetMethod,
                LargeWinHandpayResetMethod.PayByHand);
            return resetMethod == LargeWinHandpayResetMethod.PayBy1HostSystem &&
                   @this.EligibleResetToCreditMeter(propertiesManager, bank) &&
                   (@this.State == HandpayState.Pending || @this.State == HandpayState.Requested)
                ? ResetId.HandpayResetToTheCreditMeterIsAvailable
                : ResetId.OnlyStandardHandpayResetIsAvailable;
        }

        private static long GetPartialPayAmount(HandpayTransaction transaction, IGameHistory gameHistory)
        {
            var currentLog = gameHistory.CurrentLog;
            switch (transaction.HandpayType)
            {
                case HandpayType.GameWin when currentLog != null:
                    return currentLog.CashOutInfo.Where(x => x.Reason == TransferOutReason.CashWin)
                        .Sum(x => x.Amount);
                default:
                    return 0L;
            }
        }

        private static (LevelId level, uint progressiveGroup) GetProgressiveHandpayInfo(
            HandpayTransaction transaction,
            IGameHistory gameHistory,
            IProgressiveWinDetailsProvider progressiveWinDetailsProvider)
        {
            var currentLog = gameHistory.CurrentLog;
            switch (transaction.HandpayType)
            {
                case HandpayType.GameWin when currentLog != null && currentLog.Jackpots.Any():
                    var progressiveWinDetails = progressiveWinDetailsProvider.GetProgressiveWinDetails(currentLog);
                    return ((LevelId)progressiveWinDetails.LevelId, (uint)progressiveWinDetails.GroupId);
                case HandpayType.GameWin:
                case HandpayType.BonusPay:
                    return (LevelId.NonProgressiveWin, NonLinkedProgressiveWinGroupId);
                case HandpayType.CancelCredit:
                    return (LevelId.HandpayCanceledCredits, NonLinkedProgressiveWinGroupId);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}