namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Bingo player event.
    /// </summary>
    public class BingoPlayerEvent : BaseEvent
    {
        public BingoPlayerEvent(bool playerFound)
        {
            PlayerFound = playerFound;
        }

        public bool PlayerFound { get; }
    }
}
