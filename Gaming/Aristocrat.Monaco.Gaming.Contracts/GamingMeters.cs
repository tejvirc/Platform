namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Gaming Meters
    /// </summary>
    public static class GamingMeters
    {
        /// <summary>The wagered amount</summary>
        public const string WageredAmount = "WageredAmount";

        /// <summary>The wagered cashable credit amount</summary>
        public const string WageredCashableAmount = "WageredCashableAmount";

        /// <summary>The wagered promotional credit amount</summary>
        public const string WageredPromoAmount = "WageredPromoAmount";

        /// <summary>
        ///     The wagered non-cashable amount
        /// </summary>
        public const string WageredNonCashableAmount = "WageredNonCashableAmount";

        /// <summary>The count of games played</summary>
        public const string PlayedCount = "PlayedCount";

        /// <summary>The count of games won</summary>
        public const string WonCount = "WonCount";

        /// <summary>The count of games lost</summary>
        public const string LostCount = "LostCount";

        /// <summary>The count of games tied</summary>
        public const string TiedCount = "TiedCount";

        /// <summary>The count of primary games where no outcome could be determined as the game failed to complete.</summary>
        public const string FailedCount = "FailedCount";

        /// <summary>The amount won</summary>
        public const string EgmPaidGameWonAmount = "EgmPaidGameWonAmt";

        /// <summary>The amount won</summary>
        public const string TotalEgmPaidGameWonAmount = "TotalEgmPaidGameWonAmt";

        /// <summary>The total amount hand paid for a non progressive win</summary>
        public const string HandPaidGameWonAmount = "HandPaidGameWonAmt";

        /// <summary>The total amount hand paid for a non progressive win</summary>
        public const string TotalHandPaidGameWonAmount = "TotalHandPaidGameWonAmt";

        /// <summary>The total amount egm paid for a progressive win</summary>
        public const string EgmPaidProgWonAmount = "EgmPaidProgWonAmt";

        /// <summary>The total amount hand paid for a progressive win</summary>
        public const string HandPaidProgWonAmount = "HandPaidProgWonAmt";

        /// <summary>The total amount hand paid for a win</summary>
        public const string HandPaidTotalWonAmount = "HandPaidTotalWonAmt";

        /// <summary>The total EGM paid amount</summary>
        public const string TotalEgmPaidAmt = "TotalEgmPaidAmt";

        /// <summary>The total hand and attendant paid amount</summary>
        public const string TotalHandPaidAmt = "TotalHandPaidAmt";

        /// <summary>The total paid amount (Egm + hand paid)</summary>
        public const string TotalPaidAmt = "TotalPaidAmt";

        /// <summary>The total paid amount via linked progressive jackpot wins</summary>
        public const string TotalPaidLinkedProgWonAmt = "TotalPaidLinkedProgWonAmt";

        /// <summary>TotalPaidAmt that excludes all wins paid from linked progressive hits</summary>
        public const string TotalPaidAmtExcludingTotalPaidLinkedProgAmt = "TotalPaidAmtExcludingTotalPaidLinkedProgAmt";

        /// <summary>The count of games played since the door was closed</summary>
        public const string GamesPlayedSinceDoorClosed = "GamesPlayedSinceDoorClosed";

        /// <summary>The count of games played since the door was open</summary>
        public const string GamesPlayedSinceDoorOpen = "GamesPlayedSinceDoorOpen";

        /// <summary>The count of games played since reboot</summary>
        public const string GamesPlayedSinceReboot = "GamesPlayedSinceReboot";

        /// <summary>The theoretical payback amount</summary>
        /// <remarks>The amount wagered times the payback percentage of the wager category played.</remarks>
        public const string TheoPayback = "TheoPayback";

        /// <summary>The weighted average theoretical payback percentage</summary>
        public const string AveragePayback = "AvgPayback";

        /// <summary>The wagered amount per wager category</summary>
        public const string WagerCategoryWageredAmount = "WagerCategory.WageredAmount";

        /// <summary>The count of games played per wager category</summary>
        public const string WagerCategoryPlayedCount = "WagerCategory.PlayedCount";

        /// <summary>The win amount for the primary game</summary>
        public const string PrimaryWonAmount = "PrimaryWonAmount";

        /// <summary>The wagered amount for the secondary game</summary>
        public const string SecondaryWageredAmount = "SecondaryWageredAmount";

        /// <summary>The win amount for the secondary game</summary>
        public const string SecondaryWonAmount = "SecondaryWonAmount";

        /// <summary>The games played count for the secondary game</summary>
        public const string SecondaryPlayedCount = "SecondaryPlayedCount";

        /// <summary>The win count for the secondary game</summary>
        public const string SecondaryWonCount = "SecondaryWonCount";

        /// <summary>The loss count for the secondary game</summary>
        public const string SecondaryLostCount = "SecondaryLostCount";

        /// <summary>The tie count for the secondary game</summary>
        public const string SecondaryTiedCount = "SecondaryTiedCount";

        /// <summary>The total number of won and tied count for the secondary game</summary>
        public const string SecondaryTiedAndWonCount = "SecondaryTiedAndWonCount";

        /// <summary>The count of secondary games where no outcome could be determined as the game failed to complete.</summary>
        public const string SecondaryFailedCount = "SecondaryFailedCount";

        /// <summary>Tournament Games played</summary>
        public const string TournamentGamesPlayed = "TournamentGamesPlayed";

        /// <summary>Tournament Games won</summary>
        public const string TournamentGamesWon = "TournamentGamesWon";

        /// <summary>Tournament credits wagered</summary>
        public const string TournamentCreditsWagered = "TournamentCreditsWagered";

        /// <summary>Tournament credits won</summary>
        public const string TournamentCreditsWon = "TournamentCreditsWon";

        /// <summary>The bonus amount won for cashable credits</summary>
        public const string EgmPaidBonusCashableInAmount = "EgmPaidBonusCashableInAmount";

        /// <summary>The bonus amount won for non-cashable credits</summary>
        public const string EgmPaidBonusNonCashInAmount = "EgmPaidBonusNonCashInAmount";

        /// <summary>The bonus amount won for promotional credits</summary>
        public const string EgmPaidBonusPromoInAmount = "EgmPaidBonusPromoInAmount";

        /// <summary>The bonus amount won for all credits</summary>
        public const string EgmPaidBonusAmount = "EgmPaidBonusAmount";

        /// <summary>The bonus amount won for cashable credits</summary>
        public const string HandPaidBonusCashableInAmount = "AttendantPaidBonusCashableInAmount";

        /// <summary>The bonus amount won for non-cashable credits</summary>
        public const string HandPaidBonusNonCashInAmount = "AttendantPaidBonusNonCashInAmount";

        /// <summary>The bonus amount won for promotional credits</summary>
        public const string HandPaidBonusPromoInAmount = "AttendantPaidBonusPromoInAmount";

        /// <summary>The bonus amount jackpot won for all credits</summary>
        public const string HandPaidBonusAmount = "AttendantPaidBonusAmount";

        /// <summary>The machine paid bonus amount for wager match mode</summary>
        public const string EgmPaidBonusWagerMatchAmount = "EgmPaidBonusWagerMatchAmount";

        /// <summary>The attendant paid bonus amount for wager match mode</summary>
        public const string HandPaidBonusWagerMatchAmount = "AttendantPaidBonusWagerMatchAmount";

        /// <summary>The total bonus amount for wager match mode</summary>
        public const string WagerMatchBonusAmount = "WagerMatchBonusAmount";

        /// <summary>The total count for wager match mode</summary>
        public const string WagerMatchBonusCount = "WagerMatchBonusCount";

        /// <summary>The number of times a progressive jackpot was hand paid</summary>
        public const string HandPaidProgWonCount = "HandPaidProgWonCount";

        /// <summary>The number of times an attendant paid jackpot was won</summary>
        public const string TotalJackpotWonCount = "TotalJackpotWonCount";

        /// <summary>The number of times a large paytable win was hand paid</summary>
        public const string HandPaidGameWonCount = "HandPaidGameWonCount";

        /// <summary>The number of times a progressive jackpot was machine paid</summary>
        public const string EgmPaidProgWonCount = "EgmPaidProgWonCount";

        /// <summary>The number of times a progressive jackpot was won</summary>
        public const string TotalProgWonCount = "TotalProgWonCount";

        /// <summary>The machine paid bonus amount for deductible mode</summary>
        public const string EgmPaidBonusDeductibleAmount = "EgmPaidBonusDeductibleAmount";

        /// <summary>The machine paid bonus count for deductible mode</summary>
        public const string EgmPaidBonusDeductibleCount = "EgmPaidBonusDeductibleCount";

        /// <summary>The machine paid bonus amount for non deductible mode</summary>
        public const string EgmPaidBonusNonDeductibleAmount = "EgmPaidBonusNonDeductibleAmount";

        /// <summary>The machine paid bonus count for non deductible mode</summary>
        public const string EgmPaidBonusNonDeductibleCount= "EgmPaidBonusNonDeductibleCount";

        /// <summary>The attendant paid bonus amount for deductible mode</summary>
        public const string HandPaidBonusDeductibleAmount = "AttendantPaidBonusDeductibleAmount";

        /// <summary>The attendant paid bonus count for deductible mode</summary>
        public const string HandPaidBonusDeductibleCount = "AttendantPaidBonusDeductibleCount";

        /// <summary>The attendant paid bonus amount for non deductible mode</summary>
        public const string HandPaidBonusNonDeductibleAmount = "AttendantPaidBonusNonDeductibleAmount";

        /// <summary>The attendant paid bonus count for non deductible mode</summary>
        public const string HandPaidBonusNonDeductibleCount = "AttendantPaidBonusNonDeductibleCount";

        /// <summary>Bet Keeper Games Not Awarded</summary>
        public const string BetKeeperGamesNotAwarded = "BetKeeperGamesNotAwarded";

        /// <summary>Bet Keeper Games Awarded</summary>
        public const string BetKeeperGamesAwarded = "BetKeeperGamesAwarded";

        /// <summary>Bet Keeper Games Played</summary>
        public const string BetKeeperGamesPlayed = "BetKeeperGamesPlayed";
    }
}