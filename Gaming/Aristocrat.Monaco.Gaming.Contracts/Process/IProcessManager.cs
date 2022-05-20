namespace Aristocrat.Monaco.Gaming.Contracts.Process
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    ///     Provides a mechanism to control a game process.
    /// </summary>
    public interface IProcessManager
    {
        /// <summary>
        ///     Starts a process
        /// </summary>
        /// <param name="processInfo">The <see cref="ProcessStartInfo" /> that describes the file to be started</param>
        /// <returns>The process id if successful or -1</returns>
        int StartProcess(ProcessStartInfo processInfo);

        /// <summary>
        ///     Starts a process
        /// </summary>
        /// <param name="path">The path to the process</param>
        /// <param name="args">The initialization data provided to the process at startup</param>
        /// <returns>The process id if successful or -1</returns>
        int StartProcess(string path, IProcessArgs args);

        /// <summary>
        ///     Ends the specified process
        /// </summary>
        /// <param name="processId">The process identifier</param>
        /// <param name="notifyExited">
        ///     true if the process is to be treated as a hung process.  This will result in no events being
        ///     emitted for process termination.
        /// </param>
        void EndProcess(int processId, bool notifyExited = true);

        /// <summary>
        ///     Notification that the process is exiting on its own.
        /// </summary>
        void ProcessExiting();

        /// <summary>
        ///     Gets a list of running process that were started by this implementation
        /// </summary>
        /// <returns>The list of running processes</returns>
        IEnumerable<int> GetRunningProcesses();

        /// <summary>
        ///     Creates a minidump
        /// </summary>
        /// <param name="processId">The process id</param>
        void CreateMiniDump(int processId);
    }
}