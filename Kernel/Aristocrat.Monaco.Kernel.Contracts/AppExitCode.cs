namespace Aristocrat.Monaco.Kernel.Contracts
{
    /// <summary>
    ///     Exit codes are in-sync with the Platform Launcher codebase.
    ///     See. https://github.com/Aristocrat-Monaco-Platform/platform-launcher/blob/main/Platform.Launcher/Program.cs
    /// </summary>
    /// <remarks>
    ///     The Platform launcher uses these exit codes and acts accordingly after the Platform has exited.
    /// </remarks>
    public enum AppExitCode
    {
        /// <summary>
        ///     Win32 Software OS Reboot
        /// </summary>
        Reboot = -2,

        /// <summary>
        ///     Platform and Launcher exit into Aristocrat Shell
        /// </summary>
        Shutdown = -1,

        /// <summary>
        ///     Launcher acts as a watchdog and relaunches the Platform
        /// </summary>
        Ok = 0,

        /// <summary>
        ///     Launcher acts as a watchdog and relaunches the Platform
        /// </summary>
        Error = 1
    }
}