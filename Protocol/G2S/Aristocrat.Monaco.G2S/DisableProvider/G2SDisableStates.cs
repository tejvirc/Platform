namespace Aristocrat.Monaco.G2S.DisableProvider
{
    using System;

    /// <summary>Definition of the G2SDisableStates enum.</summary>
    [Flags]
    public enum G2SDisableStates
    {
        /// <summary>Not disabled.</summary>
        None = 0,

        /// <summary>Comms are offline with a G2S progressive host</summary>
        ProgressiveHostCommsOffline = 1 << 0,

        /// <summary>EGM and G2S Host mismatch on available progressive levels</summary>
        ProgressiveLevelsMismatch = 1 << 1,

        /// <summary>G2S Host has disabled a progressive device</summary>
        ProgressiveStateDisabledByHost = 1 << 2,

        /// <summary>Haven't received a valid progressive value update beyond the timer threshold</summary>
        ProgressiveValueNotReceived = 1 << 3,

        /// <summary>G2S Host has indicated EGM meters have rolled back. Non-recoverable</summary>
        ProgressiveMeterRollback = 1 << 4,
    }
}
