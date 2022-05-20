namespace Aristocrat.Monaco.G2S.Common.GAT.Models
{
    using System;

    /// <summary>
    ///     Get log status result
    /// </summary>
    public class GetLogStatusResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetLogStatusResult" /> class.
        /// </summary>
        /// <param name="lastSequence">Max value from VerificationRequests</param>
        /// <param name="totalEntries">Count records of VerificationRequests</param>
        public GetLogStatusResult(long lastSequence, int totalEntries)
        {
            if (lastSequence < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lastSequence));
            }

            if (totalEntries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalEntries));
            }

            LastSequence = lastSequence;
            TotalEntries = totalEntries;
        }

        /// <summary>
        ///     Gets the last sequence.
        /// </summary>
        /// <value>
        ///     The last sequence.
        /// </value>
        public long LastSequence { get; }

        /// <summary>
        ///     Gets the total entries.
        /// </summary>
        /// <value>
        ///     The total entries.
        /// </value>
        public int TotalEntries { get; }
    }
}