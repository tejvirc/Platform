namespace Aristocrat.Monaco.Kernel.Contracts
{
    /// <summary>
    ///     Helper used for mapping the Monaco specific <see cref="ExitAction" /> to the corresponding
    ///     <see cref="AppExitCode" /> expected by the Platform Launcher.
    /// </summary>
    public static class ExitActionToExitCodeMapper
    {
        /// <summary>
        ///     Maps a Monaco specific <see cref="ExitAction" /> to the corresponding
        ///     <see cref="AppExitCode" /> expected by the Platform Launcher.
        /// </summary>
        /// <param name="action">Application Exit Action for the Launcher to take after exit.</param>
        /// <returns>The Windows specific return code.</returns>
        public static AppExitCode Map(ExitAction action)
        {
            return action switch
            {
                ExitAction.RestartPlatform => AppExitCode.Ok,
                ExitAction.Shutdown => AppExitCode.Shutdown,
                ExitAction.RebootDevice => AppExitCode.Reboot,
                _ => AppExitCode.Error
            };
        }
    }
}