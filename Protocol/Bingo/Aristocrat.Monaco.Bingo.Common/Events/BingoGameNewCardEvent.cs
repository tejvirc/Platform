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
        public BingoGameNewCardEvent(BingoCard bingoCard)
        {
            BingoCard = bingoCard;
        }

        /// <summary>
        ///     Get the <see cref="BingoCard"/>.
        /// </summary>
        public BingoCard BingoCard { get; }
    }
}
