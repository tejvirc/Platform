namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Player meter definitions
    /// </summary>
    public static class PlayerMeters
    {
        /// <summary>
        ///     Non-cashable credits wagered on the EGM while a player session was active
        /// </summary>
        public const string CardedWageredNonCashableAmount = "CardedWageredNonCashableAmount";

        /// <summary>
        ///     Cashable credits wagered on the EGM while a player session was active
        /// </summary>
        public const string CardedWageredCashableAmount = "CardedWageredCashableAmount";

        /// <summary>
        ///     Cashable promotional credits wagered on the EGM while a player session was active
        /// </summary>
        public const string CardedWageredPromoAmount = "CardedWageredPromoAmount";

        /// <summary>
        ///     Total base paytable win from game play (EGM paid and hand paid) while a player session was active
        /// </summary>
        public const string CardedGameWonAmount = "CardedGameWonAmount";

        /// <summary>
        ///     Total progressive win (EGM paid and hand paid) while a player session was active
        /// </summary>
        public const string CardedProgressiveWonAmount = "CardedProgressiveWonAmount";

        /// <summary>
        ///     Total external bonus awards (EGM paid and hand paid) while a player session was active
        /// </summary>
        public const string CardedBonusWonAmount = "CardedBonusWonAmount";

        /// <summary>
        ///     Number of games played while player card was inserted
        /// </summary>
        public const string CardedPlayedCount = "CardedPlayedCount";
    }
}