namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     Describes data for a progressive level update.
    /// </summary>
    public class ProgressiveLevelUpdate
    {
        /// <summary>
        ///     Creates a new instance of the ProgressiveLevelUpdate class.
        /// </summary>
        /// <param name="id">The level id</param>
        /// <param name="amount">The current amount of the progressive level</param>
        /// <param name="fraction">The fractional amount associated with the level</param>
        /// <param name="recovering">Whether the system is in recovery when the update is processing</param>
        public ProgressiveLevelUpdate(
            int id,
            long amount,
            long fraction,
            bool recovering)
        {
            Id = id;
            Amount = amount;
            Fraction = fraction;
            Recovering = recovering;
        }

        /// <summary>
        ///     Gets the level id
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gets the current amount of the progressive level
        /// </summary>
        public long Amount { get; }

        /// <summary>
        ///     Gets the fractional value associated with the progressive level update
        /// </summary>
        public long Fraction { get; }

        /// <summary>
        ///     Gets a value indicating if the level update is occuring during recovery
        /// </summary>
        public bool Recovering { get; }
    }
}