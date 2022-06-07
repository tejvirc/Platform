namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using Contracts.Progressives;

    /// <summary>
    ///     Trigger jackpot command
    /// </summary>
    public class CheckMysteryJackpot
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CheckMysteryJackpot" /> class.
        /// </summary>
        /// <param name="poolName">The pool name</param>
        /// <param name="levelIds">A list of levelIds that are part of the jackpot hit</param>
        /// <param name="transactionIds">
        ///     A list of transactionIds.  If provided the transaction Ids represent one or more
        ///     transactions previously created and provided to the game.
        /// </param>
        /// <param name="recovering">true if recovering</param>
        public CheckMysteryJackpot(string poolName, IList<int> levelIds, IList<long> transactionIds, bool recovering)
        {
            PoolName = poolName;
            LevelIds = levelIds;
            TransactionIds = transactionIds;
            Recovering = recovering;
        }

        /// <summary>
        ///     Gets the pool name
        /// </summary>
        public string PoolName { get; }

        /// <summary>
        ///     Gets the level identifiers
        /// </summary>
        public IList<int> LevelIds { get; }

        /// <summary>
        ///     Gets the transaction identifiers
        /// </summary>
        public IList<long> TransactionIds { get; }

        /// <summary>
        ///     Gets a value indicating whether the game is recovering.
        /// </summary>
        /// <value>True if recovering, false if not.</value>
        public bool Recovering { get; }

        /// <summary>
        ///     Gets or sets the result of the command
        /// </summary>
        public Dictionary<uint, bool> Results { get; set; }
    }
}