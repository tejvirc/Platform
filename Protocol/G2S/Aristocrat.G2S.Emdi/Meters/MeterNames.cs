namespace Aristocrat.G2S.Emdi.Meters
{
    /// <summary>
    /// Constants for meters
    /// </summary>
    public static class MeterNames
    {
        /// <summary>
        /// Wildcard for all meters
        /// </summary>
        public const string All = "IGT_all";

        /// <summary>
        /// Total player point balance at the EGM. This meter includes
        /// the host points plus points accrued this session.
        /// </summary>
        public const string PlayerPointBalance = "IGT_playerPointBalance";

        /// <summary>
        /// EGM meter of player point countdown.
        /// </summary>
        public const string PlayerPointCountdown = "IGT_playerPointCountdown";

        /// <summary>
        /// EGM meter of player points earned this session.
        /// </summary>
        public const string PlayerSessionPoints = "IGT_playerSessionPoints";

        /// <summary>
        /// EGM meter of Total Wager Match balance across all the
        /// devices. Value is in millicents.
        /// </summary>
        public const string WagerMatchBalance = "IGT_wagerMatchBalance";

        /// <summary>
        /// Value of the cashable credit meter.
        /// </summary>
        public const string G2SPlayerCashableAmt = "G2S_playerCashableAmt";

        /// <summary>
        /// Value of promo credit meter.
        /// </summary>
        public const string G2SPlayerPromoAmt = "G2S_playerPromoAmt";

        /// <summary>
        /// Value of non-cashable credit meter.
        /// </summary>
        public const string G2SPlayerNonCashAmt = "G2S_playerNonCashAmt";
    }
}