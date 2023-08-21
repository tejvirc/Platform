namespace Aristocrat.Monaco.Bingo.Common.Events
{
    using Kernel;

    /// <summary>
    ///     This event is sent after a bingo game has ended due to
    ///     someone getting a coverall
    /// </summary>
    public class BingoGameEndedEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor for <see cref="BingoGameEndedEvent"/>
        /// </summary>
        /// <param name="gameSerialNumber">The serial number of the game</param>
        public BingoGameEndedEvent(long gameSerialNumber)
        {
            GameSerialNumber = gameSerialNumber;
        }

        /// <summary>
        ///     Get the game serial number.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public long GameSerialNumber { get; }
    }
}