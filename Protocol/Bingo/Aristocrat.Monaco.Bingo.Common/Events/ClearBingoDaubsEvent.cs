namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    /// <summary>
    ///     An event used to clear the daubs for the bingo card and ball call.
    ///     The will also clear bingo patterns if any are displayed.
    ///     The current existing ball call and bingo card numbers will be shown without any daubs
    /// </summary>
    public class ClearBingoDaubsEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor for ClearBingoDaubsEvent
        /// </summary>
        /// <param name="gameIndex">The index for the game associated with this event</param>
        public ClearBingoDaubsEvent(int gameIndex = 0)
        {
            GameIndex = gameIndex;
        }

        /// <summary>
        ///     Gets the index of the game associated with this event
        /// </summary>
        public int GameIndex { get; }
    }
}