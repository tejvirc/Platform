namespace Aristocrat.Monaco.Gaming.Runtime
{
    using Contracts.Models;

    /// <summary>
    ///     Starts and stops a game process
    /// </summary>
    public interface IGameProcess
    {
        /// <summary>
        ///     Starts the game process
        /// </summary>
        /// <param name="request">The game initialization details</param>
        /// <returns>the process id</returns>
        int StartGameProcess(GameInitRequest request);

        /// <summary>
        ///     Ends the current game process
        /// </summary>
        /// <param name="notifyExited">true if the process exit events should be emitted.</param>
        void EndGameProcess(bool notifyExited = true);

        /// <summary>
        ///     Ends the current game process
        /// </summary>
        /// <param name="processId">The process Id to end</param>
        /// <param name="notifyExited">true if the process exit events should be emitted.</param>
        void EndGameProcess(int processId, bool notifyExited = true);

        /// <summary>
        ///     Determines if the process id specified is running
        /// </summary>
        /// <param name="processId">The process Id</param>
        /// <returns>true if the process is running, otherwise false</returns>
        bool IsRunning(int processId);

        /// <summary>
        ///     Notification that the process is exiting on its own.
        /// </summary>
        void Exiting();

        /// <summary>
        ///     Creates a minidump
        /// </summary>
        void CreateMiniDump();
    }
}
