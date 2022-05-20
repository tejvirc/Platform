namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     End primary game command
    /// </summary>
    public class PrimaryGameEnded
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryGameEnded" /> class.
        /// </summary>
        /// <param name="initialWin">The initial win from primary game, including progressive win amounts</param>
        public PrimaryGameEnded(long initialWin)
        {
            InitialWin = initialWin;
        }

        /// <summary>
        ///     Gets the initial win from primary game, including progressive win amounts
        /// </summary>
        public long InitialWin { get; }
    }
}
