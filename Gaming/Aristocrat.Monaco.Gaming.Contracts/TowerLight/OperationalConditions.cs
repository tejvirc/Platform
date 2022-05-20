namespace Aristocrat.Monaco.Gaming.Contracts.TowerLight
{
    using System;

    /// <summary>
    ///     Operational conditions to determine TowerLight's flash status (Note: Some of the conditions are NOT mutually
    ///     exclusive).
    /// </summary>
    [Flags]
    public enum OperationalConditions
    {
        /// <summary>None</summary>
        None = 0,

        /// <summary>Idle (Ready to start a game)</summary>
        Idle = 1,

        /// <summary>Service button is pressed</summary>
        Service = 1 << 1,

        /// <summary>Soft errors</summary>
        SoftError = 1 << 2,

        /// <summary>Tilt or Error (other than Door Open)</summary>
        Tilt = 1 << 3,

        /// <summary>In Handpay lockup</summary>
        Handpay = 1 << 4,

        /// <summary>In Audit Menu</summary>
        AuditMenu = 1 << 5,

        /// <summary>Out of Service</summary>
        OutOfService = 1 << 6,

        /// <summary>Cancel Credit</summary>
        CancelCredit = 1 << 7
    }
}