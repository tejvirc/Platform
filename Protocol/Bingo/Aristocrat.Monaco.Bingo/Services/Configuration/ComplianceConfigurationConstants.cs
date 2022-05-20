namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    /// <summary>
    ///     Configuration constants that represent the compliance configuration settings sent from the server
    /// </summary>
    public static class ComplianceConfigurationConstants
    {
        /// <summary>
        ///     The amount of time the waiting for players message must be displayed
        /// </summary>
        public const string WaitForPlayersLength = "WaitForPlayersLength";

        /// <summary>
        ///     Change Card
        /// </summary>
        public const string ChangeCard = "ChangeCard";

        /// <summary>
        ///     The Option of whether or not the player can hide the bingo card
        /// </summary>
        public const string PlayerMayHideBingoCard = "PlayerMayHideBingoCard";

        /// <summary>
        ///     Which type of ball call service we are using
        /// </summary>
        public const string BallCallService = "BallCallService";

        /// <summary>
        ///     Which variant of bingo game play we are using
        /// </summary>
        public const string BingoType = "BingoType";

        /// <summary>
        ///     Game Ending Prize
        /// </summary>
        public const string GameEndingPrize = "GameEndingPrize";

        /// <summary>
        ///     Indicates the behavior of the bingo card display (Global)
        /// <remarks>Valid settings are: Always Display Bingo Card, Hide Bingo Card During Idle State</remarks>
        /// </summary>
        public const string DispBingoCard = "DispBingoCard";

        /// <summary>
        ///     Indicates the behavior of the bingo card when the cabinet is invative
        /// </summary>
        public const string HideBingoCardWhenInactive = "HideBingoCardWhenInactive";

        /// <summary>
        ///     Indicates the ball call service LC2003
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const string BallCallServiceLC2003 = "BallCallService-LC2003";

        /// <summary>
        ///     Indicates the continuous mode
        /// </summary>
        public const string ReadySetGoMode = "ReadySetGo";

        /// <summary>
        ///     Indicates the minimum number of wait for players milliseconds
        /// </summary>
        public const int MinWaitForPlayersMilliseconds = 0;

        /// <summary>
        ///     Indicates the maximum number of wait for players milliseconds
        /// </summary>
        public const int MaxWaitForPlayersMilliseconds = 14000;
    }
}
