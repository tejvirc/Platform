namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using Contracts.Progressives;

    /// <summary>
    ///     Claim Jackpot command
    /// </summary>
    public class ClaimJackpot
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ClaimJackpot" /> class.
        /// </summary>
        /// <param name="poolName">The pool name.</param>
        /// <param name="transactionIds">A list of transactionIds.</param>
        public ClaimJackpot(string poolName, IList<long> transactionIds)
        {
            PoolName = poolName;
            TransactionIds = transactionIds;
        }

        /// <summary>
        ///     Gets
        /// </summary>
        public string PoolName { get; }

        /// <summary>
        ///     Gets the transaction identifiers
        /// </summary>
        public IList<long> TransactionIds { get; }

        /// <summary>
        ///     Gets or sets the result of the command
        /// </summary>
        public IEnumerable<ClaimResult> Results { get; set; }
    }
}
