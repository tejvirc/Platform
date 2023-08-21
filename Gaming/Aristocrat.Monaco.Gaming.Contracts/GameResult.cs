namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Describes the possible results/outcome of game
    /// </summary>
    public enum GameResult
    {
        /// <summary>
        ///     The result of the game has not yet been determined
        /// </summary>
        None,

        /// <summary>
        ///     The game was unable to reach conclusion and failed. The net result of the primary game was the wager was returned
        ///     to the player
        /// </summary>
        Failed,

        /// <summary>
        ///     The game concluded and the primary game result was a loss for the player
        /// </summary>
        Lost,

        /// <summary>
        ///     The game concluded and the primary game result was a tie for the player
        /// </summary>
        Tied,

        /// <summary>
        ///     The game concluded and the primary game result was a win for the player
        /// </summary>
        Won
    }
}