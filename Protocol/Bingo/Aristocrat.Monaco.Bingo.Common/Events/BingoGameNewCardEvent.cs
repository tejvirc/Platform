namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using GameOverlay;
    using Kernel;

    /// <summary>
    ///     BingoGame new game event.
    /// </summary>
    public class BingoGameNewCardEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor for <see cref="BingoGameNewCardEvent"/>
        /// </summary>
        /// <param name="bingoCard">A <see cref="BingoCard"/>.</param>
        /// <param name="gameIndex">Integer index of the game being played.  Defaults to 0 for single game with no side bet.</param>
        public BingoGameNewCardEvent(BingoCard bingoCard, int gameIndex = 0)
        {
            BingoCard = bingoCard;
            GameIndex = gameIndex;
        }

        /// <summary>
        ///     Get the <see cref="BingoCard"/>.
        /// </summary>
        public BingoCard BingoCard { get; }

        /// <summary>
        ///     Gets the index of the game being played
        /// </summary>
        public int GameIndex { get; }
    }
}
