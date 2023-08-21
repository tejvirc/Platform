namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     Command for starting the primary game
    /// </summary>
    public class PrimaryGameStarted
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryGameStarted" /> class.
        /// </summary>
        /// <param name="gameId">The unique game Id</param>
        /// <param name="denomination">The denomination</param>
        /// <param name="wager">The wager amount</param>
        /// <param name="data">The recovery data</param>
        public PrimaryGameStarted(int gameId, long denomination, long wager, byte[] data)
        {
            GameId = gameId;
            Denomination = denomination;
            Wager = wager;
            Data = data;
        }

        /// <summary>
        ///     Gets the game identifier
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     Gets the denomination
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        ///     Gets the wager amount
        /// </summary>
        public long Wager { get; }

        /// <summary>
        ///     Gets the recovery data
        /// </summary>
        public byte[] Data { get; }
    }
}
