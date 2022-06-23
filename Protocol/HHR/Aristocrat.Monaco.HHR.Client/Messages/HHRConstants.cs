namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    using System;

    /// <summary>
    ///     HHR protocol constants for messages, lockups, and other things
    /// </summary>
    public static class HhrConstants
    {
        /// <summary>DeviceType Number for EGM</summary>
        public static ushort DeviceTypeNum => 2;

        /// <summary>DeviceType String for EGM</summary>
        public static string DeviceTypeStr => "GT";

        /// <summary>RetryCount for messages being sent to server</summary>
        public static ushort RetryCount => 5;

        /// <summary>Timeout in milliseconds for transaction messages.</summary>
        public static int MsgTransactionTimeoutMs => 15000;

        /// <summary>Timeout in milliseconds for GamePlayRequests.</summary>
        public static int GamePlayRequestTimeout => 45000;

        /// <summary>Game Mode to be sent to server.</summary>
        public static uint GameMode => 2;

        /// <summary>GamePlay message indicating whether game play is Manual Handicapped or Auto</summary>
        public static uint ManualHandicap => 1;

        /// <summary> ProgressiveInformation (From math input file)</summary>
        public static string ProgressiveInformation => "PW";

        /// <summary> Wager (From Math Input File),</summary>
        public static string Wager => "W";

        /// <summary> PrizeValue(From Math input file)</summary>
        public static string PrizeValue => "P";

        /// <summary> PrizeLocation(From Math input file)</summary>
        public static string PrizeLocation => "L";

        /// <summary> TildeDelimiter </summary>
        public static char TildeDelimiter => '~';

        /// <summary> AssignmentSeparator </summary>
        public static char AssignmentSeparator => '=';

        /// <summary>Retry ready to connect when ReadyToPlay from server asks us.</summary>
        public static double RetryReadyToPlay = 5000.0;

        /// <summary>Message timeouts for Startup messages.</summary>
        public static int StartupMessageTimeouts => 200000;

        /// <summary>Timeout after which Connection attempt will be made.</summary>
        public static double ReconnectTimeInMilliseconds = 3000.0;

        /// <summary>Timeout after which protocol initialization attempt will be made.</summary>
        public static double ReInitializationTimeInMilliseconds = 5000.0;

        /// <summary> The default server TCP IP address.</summary>
        public static string DefaultServerTcpIp => "10.0.3.108";

        /// <summary> The default server TCP port number.</summary>
        public static int DefaultServerTcpPort => 2059;

        /// <summary> The default server UDP port number.</summary>
        public static int DefaultServerUdpPort => 54321;

        /// <summary> The default server TCP IP address.</summary>
        public static string DefaultEncryptionKey => "xyz123";

        /// <summary> Interval at which we need to send heartbeat to Central Server.</summary>
        public static double HeartbeatInterval => 30000.0;

        /// <summary>Timeout in milliseconds after which failed request will be sent.</summary>
        public static double FailedRequestRetryTimeout => 30000.0;

        /// <summary>Credit in transaction failed disable key.</summary>
        public static Guid CreditInCmdTransactionErrorKey => new Guid("01079394-B351-471C-9D5E-F5CDFDEFDDCE");

        /// <summary>Credit out transaction failed disable key.</summary>
        public static Guid CreditOutCmdTransactionErrorKey => new Guid("76376CDB-3052-4088-9F7A-31F4091A6393");

        /// <summary>Central server offline disable key.</summary>
        public static Guid CentralServerOffline => new Guid("AF49D333-0428-48BA-89EF-75C00A525C8C");

        /// <summary>Protocol initialization in progress disable key.</summary>
        public static Guid ProtocolInitializationInProgress => new Guid("85E1AE72-9064-495E-9B19-4D0B43BC9ABC");

        /// <summary>Protocol initialization failed disable key.</summary>
        public static Guid ProtocolInitializationFailed => new Guid("73FFE566-9CCA-402B-8435-E007736F0C4E");

        /// <summary>Progressives initialization failed disable key.</summary>
        public static Guid ProgressivesInitializationFailedKey => new Guid("F7E93B5B-C9D1-4707-BA68-D3F0EA38EAEB");

        /// <summary>Prize Calculation error disable key.</summary>
        public static Guid PrizeCalculationErrorKey => new Guid("4D07551F-E721-4628-82F7-F243D8560827");

        /// <summary>Progressives initialization is in progress disable key.</summary>
        public static Guid GameConfigurationNotSupportedKey => new Guid("CD67EC5C-D0B0-480F-B700-E95E8208D0F6");

        /// <summary>Invalid games are enabled disable key.</summary>
        public static Guid GameSelectionMismatchKey => new Guid("AD80D540-018C-4391-A3EA-EDAEBD58000F");

        /// <summary>GamePlay request failed disable key.</summary>
        public static Guid GamePlayRequestFailedKey => new Guid("41E82482-A434-41B3-8157-48A410AEABC0");

        /// <summary>Bonus transaction notification failed disable key.</summary>
        public static Guid BonusCmdTransactionErrorKey => new Guid("D5121E0B-B865-4193-ABB7-4AF027F6A2C8");

        /// <summary>Game win amount transaction notification failed disable key.</summary>
        public static Guid GameWinCmdTransactionErrorKey => new Guid("B782DA38-4A47-45D2-B89D-44FE70A9EBF8");

        /// <summary>Transaction pending disable key.</summary>
        public static Guid TransactionPendingKey => new Guid("11DCEFFA-7396-4624-83EF-4ABAB7161756");

        /// <summary>Handpay amount transaction notification failed disable key.</summary>
        public static Guid HandpayTransactionErrorKey => new Guid("BD277623-D4FB-4EBF-8DD6-442F539883DE");

        /// <summary>Lockup key guid when ManualHandpay win is generated.</summary>
        public static Guid ManualHandicapWinKey => new Guid("6E300674-3936-4FFB-A5C9-A0DFC67D4C81");

        /// <summary>Lockup key guid when a display connection status changes</summary>
        public static Guid DisplayConnectionChangedRestartRequiredKey => new Guid("747C1541-20F9-4BE5-867A-003DD2554E09");

        /// <summary>Setting value for Quick-Pick manual handicap mode.</summary>
        public static string QuickPickMode => "Quick-Pick";

        /// <summary>Setting value for Auto-Pick manual handicap mode.</summary>
        public static string AutoPickMode => "Auto-Pick";

        /// <summary>Setting value for auto-detecting manual handicap mode.</summary>
        public static string DetectPickMode => "Auto Detect";
    }
}