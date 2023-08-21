namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Provides a service for game recovery.
    /// </summary>
    public interface IGameRecovery
    {
        /// <summary>
        ///     Gets a value indicating whether we are recovering a game.
        /// </summary>
        bool IsRecovering { get; }

        /// <summary>
        ///     Gets the id of the current or last game we tried to recover.
        /// </summary>
        int GameId { get; }

        /// <summary>
        ///     Try to starts a game recovery.
        /// </summary>
        /// <param name="gameId">The game id to try and recover.</param>
        /// <param name="verifyState">true if the current game play state should be verified.</param>
        /// <returns>True if we can start recovery.</returns>
        bool TryStartRecovery(int gameId, bool verifyState);

        /// <summary>
        ///     From a platform perspective, recovery ends when the EndGameCommandHandler
        ///     is called for the recovered game.
        /// </summary>
        void EndRecovery();

        /// <summary>
        ///     Aborts a recovery.  This is called if the platform has a legitimate reason
        ///     to interrupt a game in the middle of recovery.  An example of this is if
        ///     a lockup is generated during recovery, and the use does a replay--the replay
        ///     will need to abort the recovery.  An abort should not count as a recovery retry
        ///     since it didn't fail due to error.  When possible, the platform should restart
        ///     the recovery.
        /// </summary>
        void AbortRecovery();
    }
}