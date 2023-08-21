namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     Replay game round end command
    /// </summary>
    public class ReplayGameEnded
    {
        /// <summary>
        ///     Constructor for the replay game end command
        /// <param name="endCredits">The credits (cents) at the end of the replay game</param>
        /// </summary>
        public ReplayGameEnded(long endCredits)
        {
            EndCredits = endCredits;
        }

        /// <summary>
        ///     The end credits (cents) of the replay game
        /// </summary>
        public long EndCredits { get; }
    }
}
