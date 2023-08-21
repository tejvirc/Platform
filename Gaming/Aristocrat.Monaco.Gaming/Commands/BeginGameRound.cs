namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     Begin game round command
    /// </summary>
    public class BeginGameRound
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BeginGameRound" /> class.
        /// </summary>
        /// <param name="denom">The denom of the game round.</param>
        public BeginGameRound(long denom)
        {
            Denom = denom;
        }

        /// <summary>
        ///     Gets the selected denomination.
        /// </summary>
        public long Denom { get; }

        /// <summary>
        ///     Gets a value indicating whether or not the game round can start
        /// </summary>
        public bool Success { get; set; }
    }
}