namespace Aristocrat.Monaco.G2S
{
    using System;

    /// <summary>
    ///     GS2 Constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        ///     Default port for the G2S client.
        /// </summary>
        public const int DefaultPort = 53302;

        /// <summary>
        ///     Default value for NoProgressiveInfo value for progressive devices.
        ///     This is the time in milliseconds between required progressive value updates. 
        /// </summary>
        public const int DefaultNoProgInfoTimeout = 30_000;

        /// <summary>
        ///     Property to get the configured port
        /// </summary>
        public const string Port = @"G2SClient.Port";

        /// <summary>
        ///     Firewall rule name for the G2S client port
        /// </summary>
        public const string FirewallRuleName = "G2S inbound TCP Port";

        /// <summary>
        ///     G2S Api path
        /// </summary>
        public const string ResourcePath = @"/ati/api/G2S";

        /// <summary>
        ///     The properties manager cabinet style position key.
        /// </summary>
        public const string CabinetStyle = @"Cabinet.CabinetStyleString";

        /// <summary>
        ///     The reporting denomination identifier.
        /// </summary>
        public const string ReportDenomId = @"Cabinet.ReportDenomId";

        /// <summary>
        ///     The available game play profiles
        /// </summary>
        public const string GameProfiles = @"GamePlay.Profiles";

        /// <summary>
        ///     The currently selected game
        /// </summary>
        public const string SelectedGameId = @"GamePlay.SelectedDeviceId";

        /// <summary>
        ///     The currently selected denomination
        /// </summary>
        public const string SelectedDenom = @"GamePlay.SelectedDenom";

        /// <summary>
        ///     The default cabinet style.
        /// </summary>
        public const string DefaultCabinetStyle = @"G2S_upRight";

        /// <summary>
        ///     Path lookup of the games folder
        /// </summary>
        public const string GamesPath = @"/Games";

        /// <summary>
        ///     Path lookup of the database folder
        /// </summary>
        public const string DataPath = @"/Data";

        /// <summary>
        ///     Database file name
        /// </summary>
        public const string DatabaseFileName = @"protocol.sqlite";

        /// <summary>
        ///     The G2S Egm Identifier
        /// </summary>
        public const string EgmId = @"G2S.EgmId";

        /// <summary>
        ///     A collection of configured hosts
        /// </summary>
        public const string RegisteredHosts = @"G2S.RegisteredHosts";

        /// <summary>
        ///     The startup context to be used when the protocol layer starts
        /// </summary>
        public const string StartupContext = @"G2S.StartupContext";

        /// <summary>
        ///     The maximum number of hosts
        /// </summary>
        public const int MaxHosts = 8;

        /// <summary>
        ///     The maximum credit meter limit in millicents (9 9's).
        /// </summary>
        public const long DefaultMaxCreditMeter = 999999999000L;

        /// <summary>
        ///     The default certificate.
        /// </summary>
        public const string DefaultCertificate = @"egm.pfx";

        /// <summary>
        ///     Thumbprint for the default certificate.
        /// </summary>
        public const string DefaultCertificateThumbprint = @"4c26ff2a1a12f80aebd5547174efa44d557bcf36";

        /// <summary>
        ///     Password for the default certificate.
        /// </summary>
        /// <remarks>
        ///     No this isn't secure, but neither is using a self-signed cert like egm.pfx for transport security.
        /// </remarks>
        public const string CertificatePassword = @"9}S]%NpXw[,yYzB@5RXF";

        /// <summary>
        ///     Password for the protocol database.
        /// </summary>
        /// <remarks>
        ///     No this isn't secure
        /// </remarks>
        public const string DatabasePassword = @"tk7tjBLQ8GpySFNZTHYD";

        /// <summary>
        ///     The reporting denomination identifier.
        /// </summary>
        public const long DefaultReportDenomId = 1000L;

        /// <summary>
        ///     40-character data type for voucher titles.
        /// </summary>
        public const int VoucherTitle40 = 40;

        /// <summary>
        ///     16-character data type for voucher titles.
        /// </summary>
        public const int VoucherTitle16 = 16;

        /// <summary>
        ///     Gets the application id used for certificate binding.
        /// </summary>
        public static Guid ApplicationId => new Guid("{70A4153C-C65D-4D48-812D-8AD3147FDEBD}");

        /// <summary>
        ///     Gets the ISystemDisableManager key used when the G2S protocol locks the EGM during initialization
        /// </summary>
        public static Guid ProtocolDisabledKey => new Guid("{282EBE50-7A70-42C8-979F-26C2CE438290}");

        /// <summary>
        ///     Gets the ISystemDisableManager key used when the G2S protocol detects that the Vertex Progressive Host has gone offline
        /// </summary>
        public static Guid VertexOfflineKey => new Guid("{0596f5ad-9704-414c-bcd5-7806dfe8f8af}");

        /// <summary>
        ///     Gets the ISystemDisableManager key used when the G2S protocol disagrees with EGM progressive configuration 
        /// </summary>
        public static Guid VertexLevelMismatchKey => new Guid("{a2d68208-40bb-49a9-93d3-905c3d3bc072}");

        /// <summary>
        ///     Gets the ISystemDisableManager key used when the G2S protocol has disabled a progressive device
        /// </summary>
        public static Guid VertexStateDisabledKey => new Guid("{509B31C0-D4E7-490D-A60C-99B7CCFA6EF0}");

        /// <summary>
        ///     Gets the ISystemDisableManager key used when the EGM hasn't received a progressive value update within the configured time limit
        /// </summary>
        public static Guid VertexUpdateNotReceivedKey => new Guid("{54A8209A-C816-45DA-905F-1E83404BEDF2}");

        /// <summary>
        ///     Gets the ISystemDisableManager key used when the G2S protocol indicates the EGM meters have rolled back
        /// </summary>
        public static Guid VertexMeterRollbackKey => new Guid("{D07A5B23-0BE2-4E0B-AEAD-3F1FE2F9D023}");

        /// <summary>
        ///     Default value for Handpay local key-off options.
        /// </summary>
        public const string DefaultLocalKeyoff = "G2S_anyKeyOff";

        /// <summary>
        ///     The stored values for the vertex progressive level IDs.
        /// </summary>
        /// <remarks>
        ///     Currently vertex is only functional via G2S progressives, if this changes in the future then this may be stored elsewhere.
        /// </remarks>
        public const string VertexProgressiveLevelIds = "G2S_vertexLevelIds";

        /// <summary>
        ///     The key to the boolean value that represents whether or not G2S progressives are enabled.
        /// </summary>
        public const string G2SProgressivesEnabled = "G2S_progressivesEnabled";
    }
}
