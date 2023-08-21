namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using Runtime.Client;

    /// <summary>
    ///     Command used to increment jackpot values
    /// </summary>
    public class IncrementJackpotValues
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IncrementJackpotValues" /> class.
        /// </summary>
        /// <param name="poolName">The pool name</param>
        /// <param name="values">The current jackpot values</param>
        /// <param name="recovering">true is recovering</param>
        public IncrementJackpotValues(string poolName, IDictionary<uint, PoolIncrement> values, bool recovering)
        {
            PoolName = poolName;
            Values = values;
            Recovering = recovering;
        }

        /// <summary>
        ///     Gets the pool name used to lookup the jackpot values
        /// </summary>
        public string PoolName { get; }

        /// <summary>
        ///     Gets the meter values associated with the command
        /// </summary>
        public IDictionary<uint, PoolIncrement> Values { get; }

        /// <summary>
        ///     Gets a value indicating whether the increment was called while recovering
        /// </summary>
        public bool Recovering { get; }
    }
}
