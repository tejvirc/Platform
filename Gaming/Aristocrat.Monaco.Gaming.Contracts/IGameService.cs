namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Hardware.Contracts.Audio;
    using Models;

    /// <summary>
    ///     Contract for interacting with a game instance.
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        ///     Gets a value indicating whether gets a value indicating whether the game is running or not.
        /// </summary>
        bool Running { get; }

        /// <summary>
        ///     Initializes a game instance based on the specified parameters.
        /// </summary>
        /// <param name="request">The game initialization request</param>
        void Initialize(GameInitRequest request);

        /// <summary>
        ///     ReInitialize a game instance based on the specified parameters.
        /// </summary>
        /// <param name="request">The game initialization request</param>
        void ReInitialize(GameInitRequest request);

        /// <summary>
        ///     Terminates the current game instance/process.
        /// </summary>
        /// <param name="processId">The process id to terminate</param>
        void Terminate(int processId);

        /// <summary>
        ///     Terminates the current game instance/process.
        /// </summary>
        /// <param name="processId">The process id to terminate</param>
        /// <param name="notifyExited">true if the process exit events should be emitted.</param>
        void Terminate(int processId, bool notifyExited);

        /// <summary>
        ///     Terminates any running game instance/process.
        /// </summary>
        void TerminateAny();

        /// <summary>
        ///     Terminates any running game instance/process.
        /// </summary>
        /// <param name="notifyExited">true if the process exit events should be emitted.</param>
        void TerminateAny(bool notifyExited);

        /// <summary>
        ///     Tell game instance to shutdown itself gracefully.
        /// </summary>
        void ShutdownBegin();

        /// <summary>
        ///     Tell game instance to shutdown itself gracefully.
        /// </summary>
        void ShutdownEnd();

        /// <summary>
        ///     Tell game instance that game has connected (pre-initialized).
        /// </summary>
        void Connected();

        /// <summary>
        ///     Gets volume control instance that can be used to control the volume
        /// </summary>
        /// <returns>A <see cref="IVolume" /> instance that can be used to control the volume</returns>
        IVolume GetVolumeControl();

        /// <summary>
        ///     Creates a minidump
        /// </summary>
        void CreateMiniDump();
    }
}