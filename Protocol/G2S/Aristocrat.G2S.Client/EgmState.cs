namespace Aristocrat.G2S.Client
{
    /// <summary>
    ///     Describes the current state of the Egm.
    /// </summary>
    public enum EgmState
    {
        /// <summary>
        ///     Egm enabled and playable
        /// </summary>
        Enabled,

        /// <summary>
        ///     Operator menu active
        /// </summary>
        OperatorMode,

        /// <summary>
        ///     Meters/audit mode active
        /// </summary>
        AuditMode,

        /// <summary>
        ///     Operator menu active
        /// </summary>
        OperatorDisabled,

        /// <summary>
        ///     Locked via operator menu
        /// </summary>
        OperatorLocked,

        /// <summary>
        ///     Disabled by the transport layer
        /// </summary>
        TransportDisabled,

        /// <summary>
        ///     Disabled due to host
        /// </summary>
        HostDisabled,

        /// <summary>
        ///     Disabled due to device fault or door open
        /// </summary>
        EgmDisabled,

        /// <summary>
        ///     Locked due to device action
        /// </summary>
        EgmLocked,

        /// <summary>
        ///     Locked due to host action
        /// </summary>
        HostLocked,

        /// <summary>
        ///     Demo mode activated
        /// </summary>
        DemoMode
    }
}