namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Indicates the game is not in presentation play (not in a game-cycle), player input is enabled, player may leave the game and
    ///     browse the various games available on the EGM. Bonus awards MAY be paid while in the gameIdle state. The player may
    ///     adjust some game parameters such as lines bet or keno marks, without initiating a game-cycle.
    /// </summary>
    public class GamePresentationIdleEvent : BaseGameEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePresentationIdleEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        public GamePresentationIdleEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log)
            : base(gameId, denomination, wagerCategory, log)
        {
        }
    }
}