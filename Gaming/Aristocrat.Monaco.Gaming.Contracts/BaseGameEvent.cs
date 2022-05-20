namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     Defines a base type for all game related events
    /// </summary>
    public abstract class BaseGameEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseGameEvent" /> class.
        /// </summary>
        /// <param name="gameId">The unique game identifier</param>
        /// <param name="denomination">The selected denomination</param>
        /// <param name="wagerCategory">The selected wager category</param>
        /// <param name="log">The transaction log associated with this event</param>
        protected BaseGameEvent(int gameId, long denomination, string wagerCategory, IGameHistoryLog log)
        {
            GameId = gameId;
            Denomination = denomination;
            WagerCategory = wagerCategory;
            Log = log?.ShallowCopy();
        }

        /// <summary>
        ///     Gets the unique game identifier associated with the event
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     Gets the denomination associated with the event
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        ///     Gets the wager category associated with the event
        /// </summary>
        public string WagerCategory { get; }

        /// <summary>
        ///     Gets the transaction identifier associated with the event
        /// </summary>
        public IGameHistoryLog Log { get; }
    }
}