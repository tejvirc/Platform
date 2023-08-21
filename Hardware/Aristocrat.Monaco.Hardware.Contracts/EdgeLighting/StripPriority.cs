namespace Aristocrat.Monaco.Hardware.Contracts.EdgeLighting
{
    /// <summary>
    ///     Defines the strip Priorities of Various Components
    /// </summary>
    public enum StripPriority
    {
        /// <summary>
        ///     This is the priority for the default strip setting while boot up.
        /// </summary>
        LowPriority,

        /// <summary>
        ///     This is fixed priority for the game
        /// </summary>
        GamePriority,

        /// <summary>
        ///     Strips that are platformControlled
        /// </summary>
        PlatformControlled,

        /// <summary>
        ///     Lobby view priority
        /// </summary>
        LobbyView,

        /// <summary>
        ///     CashOut priority
        /// </summary>
        CashOut,

        /// <summary>
        ///     This is the priority for the Audit Page.
        /// </summary>
        AuditMenu,

        /// <summary>
        ///     This is the priority for the any door open.
        /// </summary>
        DoorOpen,

        /// <summary>
        ///     BarTopBottomStripDisable priority
        /// </summary>
        BarTopBottomStripDisable,

        /// <summary>
        ///     BarTopTowerLight priority
        /// </summary>
        BarTopTowerLight,

        /// <summary>
        ///     This is the priority for the Test Menu in Audit Page.
        /// </summary>
        PlatformTest,

        /// <summary>
        ///     Absolute Priority, Max value
        /// </summary>
        Absolute
    }
}