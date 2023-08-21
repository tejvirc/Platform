namespace Aristocrat.Monaco.Mgam.Services.PlayerTracking
{
    /// <summary>
    ///     Interface to handle player tracking.
    /// </summary>
    public interface IPlayerTracking
    {
        /// <summary>
        ///     Get a value indicating if the session is active.
        /// </summary>
        bool IsSessionActive { get; }

        /// <summary>
        ///     Ends the player session.
        /// </summary>
        void EndPlayerSession();

        /// <summary>
        ///     Starts the player session.
        /// </summary>
        /// <param name="playerName">The player name.</param>
        /// <param name="playerPoints">The player points.</param>
        /// <param name="promotionalInfo">The promotional info</param>
        void StartPlayerSession(string playerName, int playerPoints, string promotionalInfo);
    }
}