namespace Aristocrat.Monaco.Sas.Contracts.SASProperties
{
    using System;

    /// <summary>
    /// A bit flag of events that Aft should not be allowed to be performed during.
    /// </summary>
    [Flags]
    public enum AftDisableConditions
    {
        /// <summary>Not disabled.</summary>
        None = 0,

        /// <summary>Disabled while System is disabled.</summary>
        SystemDisabled = 1,

        /// <summary>Disabled while in operator menu.</summary>
        OperatorMenuEntered = 1 << 1,

        /// <summary>Disabled while jackpot pending.</summary>
        JackpotPending = 1 << 2,

        /// <summary>Disabled while canceled credits pending.</summary>
        CanceledCreditsPending = 1 << 3,

        /// <summary>Disabled while in operator menu.</summary>
        GameOperatorMenuEntered = 1 << 5,

        /// <summary>Disabled while waiting for key off.</summary>
        LockupForKeyOff = 1 << 6
    }
}
