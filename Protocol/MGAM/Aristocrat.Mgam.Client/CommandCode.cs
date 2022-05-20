namespace Aristocrat.Mgam.Client
{
    /// <summary>
    ///     Supported command codes.
    /// </summary>
    public enum CommandCode
    {
        /// <summary>EXIT</summary>
        Exit = 1,

        /// <summary>SHUTDOWN</summary>
        Shutdown = 2,

        /// <summary>REBOOT</summary>
        Reboot = 3,

        /// <summary>MALFORMED_MESSAGE</summary>
        MalformedMessage = 4,

        /// <summary>PLAY</summary>
        Play = 200,

        /// <summary>SIGN_MESSAGE</summary>
        SignMessage = 201,

        /// <summary>LOGOFF_PLAYER</summary>
        LogOffPlayer = 202,

        /// <summary>PROGRESSIVE_WINNER</summary>
        ProgressiveWinner = 203,

        /// <summary>LOCK</summary>
        Lock = 204,

        /// <summary>UPDATE_METERS</summary>
        UpdateMeters = 205,

        /// <summary>CLEAR_METERS</summary>
        ClearMeters = 206,

        /// <summary>PLAY_EXISTING_SESSION</summary>
        PlayExistingSession = 207,

        /// <summary>COMPUTE_CHECKSUM</summary>
        ComputeChecksum = 208,

        /// <summary>CLEAR_LOCK</summary>
        ClearLock = 209,

        /// <summary>UNPLAY</summary>
        Unplay = 210
    }
}
