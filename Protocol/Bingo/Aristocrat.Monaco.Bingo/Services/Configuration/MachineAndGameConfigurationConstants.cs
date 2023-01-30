namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Configuration constants that represent the machine and game configuration settings sent from the server
    /// </summary>
    [SuppressMessage(
        "ReSharper",
        "UnusedMember.Global",
        Justification = "These constants will be used as we properly recieve each of the server configuration settings")]
    public static class MachineAndGameConfigurationConstants
    {
        /// <summary>
        ///     Sets the machine serial for the EGM
        /// </summary>
        public const string MachineSerial = "MachineSerial";

        /// <summary>
        ///     Sets the location Id for the EGM
        /// </summary>
        public const string LocationId = "LocationId";

        /// <summary>
        ///     Sets the machine number for the EGM
        /// </summary>
        public const string MachineNumber = "MachineNumber";

        /// <summary>
        ///     Machine Type Id
        /// </summary>
        public const string MachineTypeId = "MachineTypeId";

        /// <summary>
        ///     Credits Manager
        /// </summary>
        public const string CreditsManager = "CreditsManager";

        /// <summary>
        ///     Location Zone Name
        /// </summary>
        public const string LocationZoneName = "LocationZoneName";

        /// <summary>
        ///     Location Zone Id
        /// </summary>
        public const string LocationZoneId = "LocationZoneId";

        /// <summary>
        ///     Location Bank
        /// </summary>
        public const string LocationBank = "LocationBank";

        /// <summary>
        ///     Location Position
        /// </summary>
        public const string LocationPosition = "LocationPosition";
        /// <summary>
        ///     Facade Type
        /// </summary>
        public const string FacadeType = "FacadeType";

        /// <summary>
        ///     Game Series Name
        /// </summary>
        public const string GameSeriesName = "GameSeriesName";

        /// <summary>
        ///     Payback Max
        /// </summary>
        public const string PaybackMax = "PaybackMax";

        /// <summary>
        ///     Indicates where the bingo card is displayed
        /// <remarks>Valid settings are: EGM Setting, Top Screen</remarks>
        /// </summary>
        public const string BingoCardPlacement = "BingoCardPlacement";

        /// <summary>
        ///     The games configuration attribute.  This data is in a JSON array and contains all the data needed to enable games
        /// </summary>
        public const string GamesConfigured = "Games";

        /// <summary>
        ///     Indicates the behavior of the bingo card display (EGM - will override global setting)
        /// </summary>
        public const string DispBingoCard = "DispBingoCard";

        /// <summary>
        ///     Indicates the behavior of the bingo card when the cabinet is inactive
        /// </summary>
        public const string HideBingoCardWhenInactive = "HideBingoCardWhenInactive";

        /// <summary>
        ///     Progressive
        /// </summary>
        public const string Progressive = "Progressive";

        /// <summary>
        ///     Indicates using EGM setting for bingo card placement
        /// </summary>
        public const string EgmSetting = "EGM Setting";

        /// <summary>
        ///     Indicates using top screen for bingo card placement
        /// </summary>
        public const string TopScreen = "Top Screen";

        /// <summary>
        ///     Indicates using global settings for bingo card display
        /// </summary>
        public const string UseGlobalSettings = "UseGlobal";

        /// <summary>
        ///     Indicates if side bet games are enabled
        /// </summary>
        public const string SideBetEnabled = "SideBetEnabled";
    }
}
