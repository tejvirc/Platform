namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System;

    /// <summary>
    ///     Defines data associated with linked progressive levels defined by an external host
    /// </summary>
    public class LinkedProgressiveLevel : IViewableLinkedProgressiveLevel
    {
        /// <summary>
        ///     Gets or sets the name of the protocol associated with this level. Eg. SAS, G2S, etc.
        /// </summary>
        public string ProtocolName { get; set;}

        /// <summary>
        ///     Gets or sets the group id associated with this linked progressive level. One progressive group id
        ///     could have multiple level ids. This maps to the "progressive identifier" in g2s and the group id
        ///     in SAS.
        /// </summary>
        public int ProgressiveGroupId { get; set; }

        /// <summary>
        ///     Gets or sets the level id associated with the linked progressive level
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive value sequence.
        /// </summary>
        public long ProgressiveValueSequence { get; set; }

        /// <summary>
        ///     Gets or sets the progressive value text.
        /// </summary>
        public string ProgressiveValueText { get; set; }

        /// <summary>
        ///     Gets or the name of the level. This should always be unique as it is
        ///     a concatenation of ProtocolName, ProgressiveGroupId, and LevelId.
        /// </summary>
        public string LevelName => $"{ProtocolName}, " +
                                   $"Level Id: {LevelId}, "+
                                   $"Progressive Group Id: {ProgressiveGroupId}";

        /// <summary>
        ///     Gets or sets the amount for the linked progressive level
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Gets or sets the time at which this level will expire or time out
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        ///     Gets or sets the current error status for the level
        /// </summary>
        public ProgressiveErrors CurrentErrorStatus { get; set; }

        /// <summary>
        ///     Gets or sets that status of any claims on the level
        /// </summary>
        public LinkedProgressiveClaimStatus ClaimStatus { get; set; }

        /// <inheritdoc />
        public long WagerCredits { get; set; }

        /// <summary>
        ///     Gets the string value of the progressive level
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                $"LinkedProgressiveLevel: {LevelName} Amount={Amount} Expiration={Expiration} CurrentErrorStatus={CurrentErrorStatus} {ClaimStatus}{(WagerCredits != 0 ? " WagerCredits=" + WagerCredits : "")}";
        }
    }
}