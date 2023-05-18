namespace Aristocrat.Monaco.G2S.DisableProvider
{
    using System;

    /// <summary>Definition of the G2SDisableStates enum.</summary>
    [Flags]
    public enum G2SDisableStates
    {
        /// <summary>Not disabled.</summary>
        None = 0,

        /// <summary>Comms are offline with a G2S host</summary>
        CommsOffline = 1 << 0,

        /// <summary>EGM and G2S Host mismatch on available progressive levels</summary>
        LevelMismatch = 1 << 1,

        /// <summary>There is no communication with Sas host 0.</summary>
        ProgressiveState = 1 << 2,

        /// <summary>There is no communication with Sas host 0.</summary>
        ProgressiveValueNotReceived = 1 << 3,

        /// <summary>There is no communication with Sas host 0.</summary>
        ProgressiveMeterRollback = 1 << 4,
    }
}
