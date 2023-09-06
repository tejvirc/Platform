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
        ///     Gets or sets the progressive value sequence.
        /// </summary>
        public long ProgressiveValueSequence { get; }

        /// <summary>
        ///     Gets or sets the progressive value text.
        /// </summary>
        public string ProgressiveValueText { get; }

        /// <summary>
        ///     Gets the name of the level. This should always be unique as it is
        ///     a concatenation of ProtocolName, ProgressiveGroupId, and LevelId.
        /// </summary>
        string LevelName { get; }

        /// <summary>
        ///     Gets the common name of the level, as might be displayed to a player
        ///     Used in some meter reporting.
        ///     Not changeable once created to prevent meter names changing
        /// </summary>
        string CommonLevelName { get; }

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

        /// <summary>
        ///     Gets or sets the progressive flavor type associated with this level.
        ///     This is used by vertex to distinguish various level funding behaviors
        ///     All levels mapped to this Linked level must be the same flavor type
        /// </summary>
        public FlavorType FlavorType { get;}

    }
}