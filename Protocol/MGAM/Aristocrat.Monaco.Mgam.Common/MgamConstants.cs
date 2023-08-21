namespace Aristocrat.Monaco.Mgam.Common
{
    using System;
    using Gaming.Contracts.InfoBar;

    /// <summary>
    ///     MGAM Constants.
    /// </summary>
    public static class MgamConstants
    {
        /// <summary>
        ///     Firewall rule name for the Directory service client port.
        /// </summary>
        public const string FirewallDirectoryRuleName = "MGAM inbound UDP Port";

        /// <summary>
        ///     Firewall rule name for the Directory service client port.
        /// </summary>
        public const string FirewallVltServiceRuleName = "MGAM outbound TCP Port";

        /// <summary>Manufacturer name.</summary>
        public const string ManufacturerName = @"Aristocrat";

        /// <summary>Configuration extension path.</summary>
        public const string ConfigurationExtensionPath = @"/MGAM/Configuration";

        /// <summary>
        ///     Path lookup of the database folder
        /// </summary>
        public const string DataPath = @"/Data";

        /// <summary>
        ///     Database file name
        /// </summary>
        public const string DatabaseFileName = @"Database_MGAM.sqlite";

        /// <summary>
        ///     Database password
        /// </summary>
        public const string DatabasePassword = @"tk7tjBLQ8GpySFNZTHYD";

        /// <summary>
        ///     Protocol disable key.
        /// </summary>
        public static Guid ProtocolDisabledKey { get; } = new Guid("0646F38E-6074-4357-91C4-9CCEF5F77F6C");

        /// <summary>
        ///     Game play disable key.
        /// </summary>
        public static Guid GamePlayDisabledKey { get; } = new Guid("2DF99CF9-CEB6-4DB6-9B84-6C08CC59A723");

        /// <summary>
        ///     Registration failed disable key.
        /// </summary>
        public static Guid RegistrationFailedDisabledKey { get; } = new Guid("DB4E4B44-F237-4245-83AE-64DB11470669");

        /// <summary>
        ///     Protocol commanded disable key
        /// </summary>
        public static Guid ProtocolCommandDisableKey { get; } = new Guid("{DB3C5804-B010-4AAB-8BB6-FCE4ACE4B0A8}");

        /// <summary>
        ///     Need Employee Card disable key.
        /// </summary>
        public static Guid NeedEmployeeCardGuid { get; } = new Guid("{2A2E3959-923F-4CEB-8A6D-75FBD8A8A470}");

        /// <summary>
        ///     Cashout on host offline key
        /// </summary>
        public static Guid HostOfflineGuid { get; } = new Guid("{8AC43BF3-46D9-4778-998C-AEDC2FE2A7FB}");

        /// <summary>
        ///     Configuring games disable key.
        /// </summary>
        public static Guid ConfiguringGamesGuid { get; } = new Guid("{d9c5386d-11d4-4a3c-a371-be6d4ea927e9}");

        /// <summary>
        ///     The default directory port.
        /// </summary>
        public const int DefaultDirectoryPort = 22046;

        /// <summary>
        ///     The default service name.
        /// </summary>
        public const string DefaultServiceName = @"MGAM VLT Service NY1";

        /// <summary>
        ///     The default compression threshold.
        /// </summary>
        public const int DefaultCompressionThreshold = 400;

        /// <summary>
        ///     The default voucher security limit.
        /// </summary>
        public const int DefaultVoucherSecurityLimit = 100000000;

        /// <summary>
        ///     The default voucher security limit.
        /// </summary>
        public const int DefaultSessionBalanceLimit = 60000;

        /// <summary>
        ///     Force Cashout After Game Round Key.
        /// </summary>
        public const string ForceCashoutAfterGameRoundKey = "Mgam.ForceCashoutAfterGameRound";

        /// <summary>
        ///     End Player Session After Game Round property key
        /// </summary>
        public const string EndPlayerSessionAfterGameRoundKey = "Mgam.EndPlayerSessionAfterGameRound";

        /// <summary>
        ///     Logoff Player After Game Round property key
        /// </summary>
        public const string LogoffPlayerAfterGameRoundKey = "Mgam.LogoffPlayerAfterGameRound";

        /// <summary>
        ///     Play Alarm After Game Round property key
        /// </summary>
        public const string PlayAlarmAfterGameRoundKey = "Mgam.PlayAlarmAfterGameRound";

        /// <summary>
        ///     Disable Game Play After Game Round property key
        /// </summary>
        public const string DisableGamePlayAfterGameRoundKey = "Mgam.DisableGamePlayAfterGameRound";

        /// <summary>
        ///     Enter Drop Mode After Game Round property key
        /// </summary>
        public const string EnterDropModeAfterGameRoundKey = "Mgam.EnterDropModeAfterGameRound";

        /// <summary>
        ///     Default alert volume
        /// </summary>
        public const byte DefaultAlertVolume = 100;

        /// <summary>
        ///     Default alert loop count
        /// </summary>
        public const int DefaultAlertLoopCount = 15;

        /// <summary>
        ///     Player message default text color
        /// </summary>
        public const InfoBarColor PlayerMessageDefaultTextColor = InfoBarColor.White;

        /// <summary>
        ///     Player message error text color
        /// </summary>
        public const InfoBarColor PlayerMessageErrorTextColor = InfoBarColor.Red;

        /// <summary>
        ///     Player message default background color
        /// </summary>
        public const InfoBarColor PlayerMessageDefaultBackgroundColor = InfoBarColor.Transparent;

        /// <summary>
        ///     Date format for MGAM tickets
        /// </summary>
        public const string DateFormat = "MMM dd yyyy";

        /// <summary>
        ///     Time format for MGAM tickets
        /// </summary>
        public const string TimeFormat = "hh:mmtt";
    }
}
