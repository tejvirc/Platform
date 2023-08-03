namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    /// <summary>
    ///     This event is used to disable a card when it is no longer in play.
    /// </summary>
    public class BingoGameDisableCardEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor for <see cref="BingoGameDisableCardEvent"/>
        /// </summary>
        /// <param name="gameIndex">The game index for the card being disabled</param>
        public BingoGameDisableCardEvent(int gameIndex)
        {
            GameIndex = gameIndex;
        }

        /// <summary>
        ///     Gets the game index
        /// </summary>
        public int GameIndex { get; }
    }
}
