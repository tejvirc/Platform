namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>The BonusMeterInfo class provides constants for bonus meter names.</summary>
    public static class BonusMeters
    {
        /// <summary>The prefix for all bonus meters.</summary>
        private const string BonusPrefix = "Bonus";

        /// <summary>The prefix for all attendant paid meters.</summary>
        private const string HandPaidPrefix = "AttendantPaid";

        /// <summary>The prefix for all machine paid meters.</summary>
        private const string EgmPaidPrefix = "EgmPaid";

        /// <summary>The postfix for all Amount meters.</summary>
        private const string AmountPostfix = "Amount";

        /// <summary>The postfix for all count meters.</summary>
        private const string CountPostfix = "Count";

        /// <summary>The base name of the Mjt Bonus meters.</summary>
        private const string MjtBonusName = "MjtBonus";

        /// <summary>The base name of the Cashable In Bonus meters.</summary>
        private const string CashableInName = "CashableIn";

        /// <summary>The prefix for all game win bonuses.  These bonuses are paid as paytable wins for a game</summary>
        private const string GameWinBonusPrefix = "GameWinBonus";

        /// <summary>The bonus amount won for cashable credits</summary>
        public const string BonusCashableInAmount = BonusPrefix + CashableInName + AmountPostfix;

        /// <summary>The bonus amount won for non-cashable credits</summary>
        public const string BonusNonCashableInAmount = BonusPrefix + "NonCashableIn" + AmountPostfix;

        /// <summary>The bonus amount won for promotional credits</summary>
        public const string BonusPromoInAmount = BonusPrefix + "PromoIn" + AmountPostfix;

        /// <summary>The total bonus count for all credits</summary>
        public const string BonusTotalCount = BonusPrefix + "Total" + CountPostfix;

        /// <summary>The bonus base amount won for cashable credits</summary>
        public const string BonusBaseCashableInAmount = BonusPrefix + "Base" + CashableInName + AmountPostfix;

        /// <summary>The bonus amount won for cashable credits</summary>
        public const string EgmPaidBonusGameWonAmount = EgmPaidPrefix + BonusPrefix +"GameWon" + AmountPostfix;

        /// <summary>The bonus amount won for promo and non-cash credit types</summary>
        public const string EgmPaidBonusGameNonWonAmount = EgmPaidPrefix + BonusPrefix + "GameNonWon" + AmountPostfix;

        /// <summary>The bonus amount jackpot won for promo and non-cash credit types</summary>
        public const string HandPaidBonusGameNonWonAmount = HandPaidPrefix + BonusPrefix + "GameNonWon" + AmountPostfix;

        /// <summary>The handpay bonus base amount won for cashable credits</summary>
        public const string HandPaidBonusBaseCashableInAmount = HandPaidPrefix + BonusPrefix + "Base" + CashableInName + AmountPostfix;

        /// <summary>The count of MJT games played</summary>
        public const string MjtGamesPlayedCount = "MjtGamesPlayed" + CountPostfix;

        /// <summary>The count of games that had a non-zero multiplied win</summary>
        public const string MjtBonusCount = MjtBonusName + CountPostfix;

        /// <summary>The amount awarded over base paytable win for each game that resulted in a non-zero multiplied win</summary>
        public const string MjtBonusAmount = MjtBonusName + AmountPostfix;

        /// <summary>The attendant paid count of games that had a non-zero multiplied win</summary>
        public const string HandPaidMjtBonusCount = HandPaidPrefix + MjtBonusName + CountPostfix;

        /// <summary>The attendant paid amount awarded over base paytable win for each game that resulted in a non-zero multiplied win</summary>
        public const string HandPaidMjtBonusAmount = HandPaidPrefix + MjtBonusName + AmountPostfix;

        /// <summary>The machine paid count of games that had a non-zero multiplied win</summary>
        public const string EgmPaidMjtBonusCount = EgmPaidPrefix + MjtBonusName + CountPostfix;

        /// <summary>The machine paid amount awarded over base paytable win for each game that resulted in a non-zero multiplied win</summary>
        public const string EgmPaidMjtBonusAmount = EgmPaidPrefix + MjtBonusName + AmountPostfix;

        /// <summary>The bonus count total for all credits</summary>
        public const string EgmPaidBonusCount = EgmPaidPrefix + BonusPrefix + CountPostfix;

        /// <summary>The handpaid bonus count total for all credits</summary>
        public const string HandPaidBonusCount = HandPaidPrefix + BonusPrefix + CountPostfix;

        /// <summary>The machine paid game win bonus awarded amount.  These bonuses are paid as paytable wins for a game</summary>
        public const string EgmPaidGameWinBonusAmount = EgmPaidPrefix + GameWinBonusPrefix + AmountPostfix;

        /// <summary>he machine paid game win bonus awarded count.  These bonuses are paid as paytable wins for a game</summary>
        public const string EgmPaidGameWinBonusCount = EgmPaidPrefix + GameWinBonusPrefix + CountPostfix;

        /// <summary>The handpaid game win bonus awarded amount.  These bonuses are paid as paytable wins for a game</summary>
        public const string HandPaidGameWinBonusAmount = HandPaidPrefix + GameWinBonusPrefix + AmountPostfix;

        /// <summary>The handpaid game win bonus awarded count.  These bonuses are paid as paytable wins for a game</summary>
        public const string HandPaidGameWinBonusCount = HandPaidPrefix + GameWinBonusPrefix + CountPostfix;
    }
}