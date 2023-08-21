namespace Aristocrat.G2S.Emdi.Events
{
    /// <summary>
    /// Constants for event codes
    /// </summary>
    public static class EventCodes
    {
        /// <summary>
        /// Wildcard for all events
        /// </summary>
        public const string All = "IGT_all";

        /// <summary>
        /// Call Attendant/Change Button Pressed
        /// </summary>
        public const string CallAttendantButtonPressed = "IGT_BTN100";

        /// <summary>
        /// Card Inserted
        /// </summary>
        public const string CardInserted = "IGT_IDE100";

        /// <summary>
        /// Card Removed
        /// </summary>
        public const string CardRemoved = "IGT_IDE101";

        /// <summary>
        /// Card Abandoned
        /// </summary>
        public const string CardAbandoned = "IGT_IDE102";

        /// <summary>
        /// Game Idle
        /// </summary>
        public const string GameIdle = "IGT_GME101";

        /// <summary>
        /// Game Idle
        /// </summary>
        public const string G2SGameIdle = "G2S_GME101";

        /// <summary>
        /// Primary Game Escrow
        /// </summary>
        public const string PrimaryGameEscrow = "IGT_GME102";

        /// <summary>
        /// Primary Game Escrow
        /// </summary>
        public const string G2SPrimaryGameEscrow = "G2S_GME102";

        /// <summary>
        /// Primary Game Started
        /// </summary>
        public const string PrimaryGameStarted = "IGT_GME103";

        /// <summary>
        /// Primary Game Started
        /// </summary>
        public const string G2SPrimaryGameStarted = "G2S_GME103";

        /// <summary>
        /// Primary Game Ended
        /// </summary>
        public const string PrimaryGameEnded = "IGT_GME104";

        /// <summary>
        /// Primary Game Ended
        /// </summary>
        public const string G2SPrimaryGameEnded = "G2S_GME104";

        /// <summary>
        /// Progressive Pending
        /// </summary>
        public const string ProgressivePending = "IGT_GME105";

        /// <summary>
        /// Progressive Pending
        /// </summary>
        public const string G2SProgressivePending = "G2S_GME105";

        /// <summary>
        /// Secondary Game Choice
        /// </summary>
        public const string SecondaryGameChoice = "IGT_GME106";

        /// <summary>
        /// Secondary Game Choice
        /// </summary>
        public const string G2SSecondaryGameChoice = "G2S_GME106";

        /// <summary>
        /// Secondary Game Escrow
        /// </summary>
        public const string SecondaryGameEscrow = "IGT_GME107";

        /// <summary>
        /// Secondary Game Escrow
        /// </summary>
        public const string G2SSecondaryGameEscrow = "G2S_GME107";

        /// <summary>
        /// Secondary Game Started
        /// </summary>
        public const string SecondaryGameStarted = "IGT_GME108";

        /// <summary>
        /// Secondary Game Started
        /// </summary>
        public const string G2SSecondaryGameStarted = "G2S_GME108";

        /// <summary>
        /// Secondary Game Ended
        /// </summary>
        public const string SecondaryGameEnded = "IGT_GME109";

        /// <summary>
        /// Secondary Game Ended
        /// </summary>
        public const string G2SSecondaryGameEnded = "G2S_GME109";

        /// <summary>
        /// Pay Game Results
        /// </summary>
        public const string PayGameResults = "IGT_GME110";

        /// <summary>Pay Game Results</summary>
        public const string G2SPayGameResults = "G2S_GME110";

        /// <summary>
        /// Game Ended
        /// </summary>
        public const string GameEnded = "IGT_GME111";

        /// <summary>
        /// Game Ended
        /// </summary>
        public const string G2SGameEnded = "G2S_GME111";

        /// <summary>
        /// Media Display Interface Open
        /// </summary>
        public const string G2SDisplayInterfaceOpen = "G2S_MDE100";

        /// <summary>
        /// Media Display Interface Closed
        /// </summary>
        public const string G2SDisplayInterfaceClosed = "G2S_MDE101";

        /// <summary>
        /// Media Display Device Shown
        /// </summary>
        public const string G2SDisplayDeviceShown = "G2S_MDE001";

        /// <summary>
        /// Media Display Device Hidden
        /// </summary>
        public const string G2SDisplayDeviceHidden = "G2S_MDE002";

        /// <summary>
        /// EGM State Changed
        /// </summary>
        public const string EgmStateChanged = "IGT_CBE100";

        /// <summary>
        /// Locale Changed
        /// </summary>
        public const string LocaleChanged = "G2S_CBE101";
    }
}
