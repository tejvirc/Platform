namespace Aristocrat.Sas.Client
{
    /// <summary>
    ///     Configuration data for each SAS clients. There will be 2 instances of this structure, one for
    ///     each SAS client.
    /// </summary>
    public struct SasClientConfiguration
    {
        /// <summary>True if this client handles AFT Long polls and Exceptions </summary>
        public bool HandlesAft;

        /// <summary>True if this client handles Legacy bonusing Long polls and Exceptions </summary>
        public bool HandlesLegacyBonusing;

        /// <summary>True if this client handles Validation Long polls and Exceptions </summary>
        public bool HandlesValidation;

        /// <summary>True if this client handles General control Long polls and Exceptions </summary>
        public bool HandlesGeneralControl;

        /// <summary>True if this client handles Game Start/End Exceptions </summary>
        public bool HandlesGameStartEnd;

        /// <summary>True if this client uses legacy handpay reporting</summary>
        public bool LegacyHandpayReporting;

        /// <summary>True if this client handles Progressives Long polls and Exceptions </summary>
        public bool HandlesProgressives;

        /// <summary>What the client should do if it detects a link down condition</summary>
        public LinkDownBehavior LinkDownAction;

        /// <summary>comm port used by this client</summary>
        public string ComPort;

        /// <summary>SAS address used by this client</summary>
        public byte SasAddress;

        /// <summary>Which client this is, 1 or 2</summary>
        public byte ClientNumber;

        /// <summary>Whether to discard oldest exception (or newest)</summary>
        public bool DiscardOldestException;

        /// <summary>Whether the validation type is NONE (System)</summary>
        public bool IsNoneValidation;

        /// <summary>The accounting denom for the SAS client</summary>
        public long AccountingDenom;
    }

    /// <summary>
    ///     The SAS Options that apply to both hosts.
    /// </summary>
    public struct SasOption
    { 
        /// <summary>The Aft transfer modes</summary>
        public AftModeOption AftMode;

        /// <summary>The action to take if the host disables</summary>
        public HostDisableCashoutOption HostDisableCashout;

        /// <summary>The hand pay modes supported</summary>
        public HandpayModeOption HandpayMode;

        /// <summary>The action to take if meters are cleared</summary>
        public MeterChangeModeOption MeterChangeMode;

        /// <summary>The Aft transfer limit</summary>
        public long AftTransferLimit;
    }

    /// <summary>
    ///     The action to take if the SAS link goes down for this host
    /// </summary>
    public enum LinkDownBehavior
    {
        DisableGamePlay,
        NoAction
    }

    /// <summary>
    ///     The supported AFT transfer types for this host
    /// </summary>
    public enum AftModeOption
    {
        AftInOutFullPartial,
        AftInFullPartial,
        AftOutFullPartial,
        AftInOutFullOnly,
        AftInFullOnly,
        AftOutFullOnly,
        None
    }

    /// <summary>
    ///     The action to take when the host disables
    /// </summary>
    public enum HostDisableCashoutOption
    {
        DisableGamePlay,
        NoAction
    }

    /// <summary>
    ///     Hand pay options
    /// </summary>
    public enum HandpayModeOption
    {

    }

    /// <summary>
    ///     The action to take if the meters are reset
    /// </summary>
    public enum MeterChangeModeOption
    {
        Enabled,
        Disabled
    }

    /// <summary>
    ///     The action to take if the game configuration changes
    /// </summary>
    public enum GameConfigurationChangeOption
    {
        DisableGamePlay,
        NoAction
    }
}