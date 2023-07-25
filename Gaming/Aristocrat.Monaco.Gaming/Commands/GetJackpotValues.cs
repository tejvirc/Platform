namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    ///     Get jackpot values command
    /// </summary>
    public class GetJackpotValues
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetJackpotValues" /> class.
        /// </summary>
        /// <param name="poolName">The pool name</param>
        /// <param name="recovering">true if recovering</param>
        /// <param name="gameName">Name of a game that is enabled, is optional</param>
        /// <param name="denom">Denomination we are interested in, is optional</param>
        public GetJackpotValues(string poolName, bool recovering, string gameName = null, ulong? denom = null)
        {
            PoolName = poolName;
            Recovering = recovering;
            GameName = gameName;
            Denomination = denom;

            JackpotValues = new Dictionary<int, long>();
        }

        /// <summary>
        ///     Gets the pool name used to lookup the jackpot values
        /// </summary>
        public string PoolName { get; }

        /// <summary>
        ///     Gets a value indicating whether this command is being called during recovery
        /// </summary>
        public bool Recovering { get; }

        [CanBeNull]
        public string GameName { get; }

        public ulong? Denomination { get; }

        /// <summary>
        ///     Gets the captured jackpot values for the current game round.
        /// </summary>
        /// <value>The jackpot values.</value>
        public IDictionary<int, long> JackpotValues { get; set; }
    }
}
