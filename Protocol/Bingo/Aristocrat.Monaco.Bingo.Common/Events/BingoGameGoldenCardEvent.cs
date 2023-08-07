namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    /// <summary>
    ///     This event is used to update an existing card instance to set/unset the IsGolden flag.
    /// </summary>
    public class BingoGameGoldenCardEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor for <see cref="BingoGameGoldenCardEvent"/>
        /// </summary>
        /// <param name="isGolden">Whether the card is golden</param>
        /// <param name="gameIndex">The game index for the card being updated</param>
        public BingoGameGoldenCardEvent(bool isGolden, int gameIndex)
        {
            IsGolden = isGolden;
            GameIndex = gameIndex;
        }

        /// <summary>
        ///     Gets whether the card is golden
        /// </summary>
        public bool IsGolden { get; }

        /// <summary>
        ///     Gets the game index
        /// </summary>
        public int GameIndex { get; }
    }
}
