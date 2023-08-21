namespace Aristocrat.Monaco.Hhr.Services
{
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Kernel;

    /// <summary>
    ///     Stores all of the information for an execution of the <see cref="HandpayService" /> so that it is kept separate
    ///     from the business logic. Basically this exists just to make the HandpayService code more readable.
    /// </summary>
    public class HandpayServiceContext
    {
        public readonly PrizeInformation PrizeInfo;
        public readonly decimal WinLimitRatio;
        public readonly long WinLimitCents;
        public readonly uint RaceSet1WagerCents;
        public readonly long RaceSet1TotalWinCents;
        public readonly long RaceSet1NetPrizeCents;
        public readonly uint RaceSet2WagerCents;
        public readonly long RaceSet2TotalWinCents;
        public readonly long RaceSet2NetPrizeCents;
        public readonly long RaceSet2NonProgressiveTotalWinCents;
        public readonly long RaceSet2NonProgressiveNetPrizeCents;
        public readonly long RaceSet2ProgressiveTotalWinCents;

        public long CurrentCreditMeterCents;

        /// <summary>
        ///     Constructs the <see cref="HandpayServiceContext"/> from the PrizeInformation object and a few system
        /// </summary>
        /// <param name="egmSettings">The property provider, where we can find configured IRS limits for handpay</param>
        /// <param name="prizeInfo">The prize information, from which we calculate various total and net win amounts</param>
        public HandpayServiceContext(
            IPropertiesManager egmSettings,
            PrizeInformation prizeInfo)
        {
            PrizeInfo = prizeInfo;

            WinLimitRatio = egmSettings.GetValue(
                AccountingConstants.LargeWinRatio,
                AccountingConstants.DefaultLargeWinRatio) / 100.0m;

            WinLimitCents = egmSettings.GetValue(
                AccountingConstants.LargeWinRatioThreshold,
                AccountingConstants.DefaultLargeWinRatioThreshold).MillicentsToCents();

            // Get the wagered amounts
            RaceSet1WagerCents = prizeInfo.RaceSet1Wager;
            RaceSet2WagerCents = prizeInfo.RaceSet2Wager;

            // Get the total prize for the game
            RaceSet1TotalWinCents = prizeInfo.RaceSet1AmountWon + prizeInfo.RaceSet1ExtraWinnings;
            RaceSet2TotalWinCents = prizeInfo.RaceSet2AmountWon + prizeInfo.RaceSet2ExtraWinnings;

            // Get the actual net win for the game
            RaceSet1NetPrizeCents = RaceSet1TotalWinCents - RaceSet1WagerCents;
            RaceSet2NetPrizeCents = RaceSet2TotalWinCents - RaceSet2WagerCents;

            // Get the separated total and net amounts for race set 2.
            RaceSet2NonProgressiveTotalWinCents = prizeInfo.RaceSet2AmountWonWithoutProgressives + prizeInfo.RaceSet2ExtraWinnings;
            RaceSet2NonProgressiveNetPrizeCents = RaceSet2NonProgressiveTotalWinCents - prizeInfo.RaceSet2Wager;
            RaceSet2ProgressiveTotalWinCents = prizeInfo.TotalProgressiveAmountWon;
        }
    }
}