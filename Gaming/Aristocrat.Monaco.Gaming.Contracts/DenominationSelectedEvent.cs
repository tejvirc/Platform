namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     The <see cref="DenominationSelectedEvent" /> is emitted when the selected denomination changes for a selected game.
    ///     The GameId may have changed due to the denomination change.
    /// </summary>
    public class DenominationSelectedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DenominationSelectedEvent" /> class.
        /// </summary>
        /// <param name="gameId">The selected game Id</param>
        /// <param name="denomination">The selected denomination</param>
        public DenominationSelectedEvent(int gameId, long denomination)
        {
            GameId = gameId;
            Denomination = denomination;
        }

        /// <summary>
        ///     Gets the unique identifier of the game.
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     Gets the denomination of the game.
        /// </summary>
        public long Denomination { get; }
    }
}