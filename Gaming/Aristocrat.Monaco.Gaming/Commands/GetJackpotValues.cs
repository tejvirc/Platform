namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;

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
        public GetJackpotValues(string poolName, bool recovering)
        {
            PoolName = poolName;
            Recovering = recovering;

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

        /// <summary>
        ///     Gets the captured jackpot values for the current game round.
        /// </summary>
        /// <value>The jackpot values.</value>
        public IDictionary<int, long> JackpotValues { get; set; }
    }
}
