namespace Aristocrat.Monaco.Bingo.Common
{
    using System;

    public static class BingoConstants
    {
        /// <summary>
        ///     BingoCardDimension of card.
        /// </summary>
        public const int BingoCardDimension = 5;

        /// <summary>
        ///     The number of squares on the bingo card
        /// </summary>
        public const int BingoCardSquares = BingoCardDimension * BingoCardDimension;

        /// <summary>
        ///     Index of middle row/column.
        /// </summary>
        public const int MidRowColumnIndex = 2;

        /// <summary>
        ///     Index of the free space in the case where the bingo card is represented as a one-dimension IEnumerable.
        /// </summary>
        public const int FreeSpaceIndex = 12;

        /// <summary>
        ///     Numerical value for center spot on bingo card.
        /// </summary>
        public const int CardCenterNumber = 0;

        /// <summary>
        ///     Minimum bingo ball number.
        /// </summary>
        public const int MinBall = 1;

        /// <summary>
        ///     Maximum bingo ball number.
        /// </summary>
        public const int MaxBall = 75;

        /// <summary>
        ///     Number of initial balls.
        /// </summary>
        public const int InitialBallDraw = 40;

        /// <summary>
        ///     The range of numbers in a Bingo card column
        /// </summary>
        public const int ColumnNumberRange = 15;

        /// <summary>
        ///     The default card title
        /// </summary>
        public const string DefaultCardTitle = "BINGO";

        /// <summary>
        ///     The default ball call title
        /// </summary>
        public const string DefaultBallCallTitle = "BINGO";

        /// <summary>
        ///     Color name to designate rainbow sequence.
        /// </summary>
        public const string RainbowColor = "Rainbow";

        /// <summary>
        ///     The default number of seconds to wait before displaying waiting for players
        /// </summary>
        public const int DefaultDelayStartWaitingForPlayersSeconds = 5;

        /// <summary>
        ///     The default number of seconds to wait for additional players to join the bingo game
        /// </summary>
        public const int DefaultWaitForPlayersSeconds = 15;

        /// <summary>
        ///     The default number of seconds to display no players found
        /// </summary>
        public const int DefaultNoPlayersFoundSeconds = 3;

        /// <summary>
        ///     The maximum time the game can set for the undaubed time
        /// </summary>
        public const int MaxPreDaubedTimeMs = 1000;

        /// <summary>
        ///     Used to get the bingo card placement (EGM Setting, Top Screen)
        /// </summary>
        public const string BingoCardPlacement = "BingoCardPlacement";

        /// <summary>
        ///     Used to get the global setting for displaying the bingo card
        /// </summary>
        public const string DisplayBingoCardGlobal = "DisplayBingoCardGlobal";

        /// <summary>
        ///     Used to get the EGM setting (will override global setting) for displaying the bingo card
        /// </summary>
        public const string DisplayBingoCardEgm = "DisplayBingoCardEgm";

        /// <summary>
        ///     Used to get whether or not the player can hide the bingo card
        /// </summary>
        public const string PlayerMayHideBingoCard = "PlayerMayHideBingoCard";

        /// <summary>
        ///     Indicates server setting is on
        /// </summary>
        public const string ServerSettingOn = "On";

        /// <summary>
        ///     Indicates server setting is off
        /// </summary>
        public const string ServerSettingOff = "Off";

        /// <summary>
        ///     Property name for fake delay in milliseconds; used for testing
        /// </summary>
        public const string TestingFakeDelayMs = "Testing.FakeDelayMs";

        /// <summary>
        ///     Property name for average card display interval; used for testing
        /// </summary>
        public const string TestingIntervalBeforeCardDisplayMs = "Testing.IntervalBeforeCardDisplayMs";

        /// <summary>
        ///     The default help URI formatted location.
        ///     This string requires two parameters the game title ID and paytable ID
        /// </summary>
        public const string DefaultBingoHelpUriFormat = "/gamehelp/{0}/{1}";

        /// <summary>
        ///     Used to get the bingo server version.
        /// </summary>
        public const string BingoServerVersion = "BingoServer.Version";

        /// <summary>Certificate configuration extension path.</summary>
        public const string CertificateExtensionPath = @"/NIGC/Certificate/Configuration";

        /// <summary>
        ///     Used to enable or disable GRPC logging
        /// </summary>
        public const string EnableGrpcLogging = "EnableGrpcLogging";

        /// <summary>
        ///     The Uri to use when starting the Bingo Overlay Server
        /// </summary>
        public const string BingoOverlayServerUri = "http://127.0.0.1:29576";

        /// <summary>
        ///     The default free space character
        /// </summary>
        public const string DefaultFreeSpaceCharacter = "*";

        /// <summary>
        ///     The default overlay css path
        /// </summary>
        public const string DefaultOverlayCssPath = "scenes.css";

        /// <summary>
        ///     The default display configuration path
        /// </summary>
        public const string DisplayConfigurationPath = "BingoDisplayConfiguration.xml";

        /// <summary>
        ///     The default initial overlay scene
        /// </summary>
        public const string DefaultInitialOverlayScene = "Normal";

        /// <summary>
        ///     The default time to cycle patterns
        /// </summary>
        public const int DefaultPatternCycleTime = 3;

        /// <summary>
        ///     The default connection timeout
        /// </summary>
        public static readonly TimeSpan DefaultConnectionTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     The disable key for when the bingo host is disconnected
        /// </summary>
        public static readonly Guid BingoHostDisconnectedKey = new("{F1BE3145-DF51-4C43-BAB6-F0E934681C74}");

        /// <summary>
        ///     The disable key for when the bingo host fails registration
        /// </summary>
        public static readonly Guid BingoHostRegistrationFailedKey = new("{037FCF2D-10B7-4615-AF08-D843887D8A86}");

        /// <summary>
        ///     The disable key for when the bingo host configuration is invalid
        /// </summary>
        public static readonly Guid BingoHostConfigurationInvalidKey = new("{C4FE7753-0CA8-4D80-AA34-C15302926043}");

        /// <summary>
        ///     The disable key for when the bingo host help url is invalid or unable to be reached
        /// </summary>
        public static readonly Guid BingoHostHelpUrlInvalidKey = new("{BA6E51D9-586F-4939-B94C-1A8017DC1776}");

        /// <summary>
        ///     The disable key for when the bingo host configuration has changed
        /// </summary>
        public static readonly Guid BingoHostConfigurationMismatchKey = new("{4E02978D-B70E-4768-AE87-3CEAFFEF6FF5}");

        /// <summary>
        ///     The disable key for when bingo server win amount does not match the final win amount
        /// </summary>
        public static readonly Guid BingoWinMismatchKey = new("{7FE13387-0F27-46CF-B385-831F5E56BD90}");

        /// <summary>
        ///     The disable key for when bingo Transaction queue is almost full
        /// </summary>
        public static readonly Guid TransactionQueueDisableKey = new("{433C53BA-4ED0-44C7-82F6-31143D1B389B}");

        /// <summary>
        ///     The disable key for when bingo Event queue is almost full
        /// </summary>
        public static readonly Guid EventQueueDisableKey = new("{F1AF9F05-CD65-4754-B2CC-2FBA94F3BE83}");

        /// <summary>
        ///     The disable key for when required bingo settings are not sent from the server
        /// </summary>
        public static readonly Guid MissingSettingsDisableKey = new("{E36644F8-1179-4D94-ADDC-D57601BB3694}");

        /// <summary>
        ///     The disable key for when game history report queue is almost full
        /// </summary>
        public static readonly Guid GameHistoryQueueDisableKey = new("{E8B1A368-EA55-4C80-8297-852D156ED314}");

        /// <summary>
        ///     Indicates if side bet games are enabled
        /// </summary>
        public const string SideBetEnabled = "SideBetEnabled";

        /// <summary>
        ///     key to use for SideBetEnabled property
        /// </summary>
        public const string SideBetEnabledKey = "SideBetEnabled";

    }
}