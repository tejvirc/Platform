namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System;

    /// <summary>
    ///     An interface that provides a read-only view into a <see cref="LinkedProgressiveLevel"/>
    /// </summary>
    public interface IViewableLinkedProgressiveLevel
    {
        /// <summary>
        ///     Gets the name of the protocol associated with this level. Eg. SAS, G2S, etc.
        /// </summary>
        string ProtocolName { get; set; }

        /// <summary>
        ///     Gets a progressive group id for the linked progressive level. This maps to
        ///     the "progressive identifier" in G2S and the "Group ID" in SAS.
        /// </summary>
        int ProgressiveGroupId { get; }

        /// <summary>
        ///     Gets the level id associated with the linked progressive level
        /// </summary>
        int LevelId { get; }

        /// <summary>
        ///     Gets or sets the level id provided by the protocol
        /// </summary>
        int ProtocolLevelId { get; }

        /// <summary>
        ///     Gets level Id to display on UI based on ProtocolName
        /// </summary>
        public int DisplayLevelId { get; }

        /// <summary>
        ///     Gets or the name of the level. This should always be unique as it is
        ///     a concatenation of ProtocolName, ProgressiveGroupId, and LevelId.
        /// </summary>
        string LevelName { get; }

        /// <summary>
        ///     Gets the current amount for the linked progressive level
        /// </summary>
        long Amount { get; }

        /// <summary>
        ///     Gets or sets the time at which this level will expire or time out
        /// </summary>
        DateTime Expiration { get; set; }

        /// <summary>
        ///     Gets or sets the current error status for the level
        /// </summary>
        ProgressiveErrors CurrentErrorStatus { get; }

        /// <summary>
        ///     Gets the status of any claims on the level
        /// </summary>
        LinkedProgressiveClaimStatus ClaimStatus { get; }


        /// <summary>
        ///     Gets the wager credits associated with this level.
        /// </summary>
        long WagerCredits { get; }

    }
}