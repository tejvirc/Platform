namespace Aristocrat.Monaco.Kernel.Contracts
{
    /// <summary>
    ///     Indicates the exit action
    /// </summary>
    public enum ExitAction
    {
        /// <summary>
        ///     Shuts down the system and then restarts it.  This is the fastest option and should be used in most cases.
        ///     NOTE: It will not verify the software before restarting the platform.
        /// </summary>
        Restart = 0,

        /// <summary>
        ///     Indicates a graceful shutdown of the entire platform including the Platform Launcher.
        ///     This option requires a separate system to restart the platform. i.e. the Windows Shell
        /// </summary>
        ShutDown = 1,

        /// <summary>
        ///     Shuts down the system including the OS and then restarts the system
        /// </summary>
        Reboot = 2
    }
}