namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;

    /// <summary>
    ///     The nudge reels command
    /// </summary>
    [Serializable]
    public class Nudge : GdsSerializableMessage, IEquatable<Nudge>, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="Nudge"/>
        /// </summary>
        public Nudge()
            : base(GdsConstants.ReportId.ReelControllerNudge)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event</summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary> Gets or sets number of reels being spun</summary>
        [FieldOrder(1)]
        public byte ReelCount { get; set; }

        /// <summary> Gets or sets nudge reel data</summary>
        [FieldOrder(2)]
        [FieldCount(nameof(ReelCount))]
        public GdsNudgeReelData[] NudgeReelData { get; set; }

        /// <inheritdoc />
        public bool Equals(Nudge other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                    TransactionId == other.TransactionId &&
                    ReelCount == other.ReelCount &&
                    NudgeReelData == other.NudgeReelData);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((Nudge)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)ReelCount;
                hashCode = (hashCode * 397) ^ NudgeReelData.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionId.GetHashCode();
                return hashCode;
            }
        }
    }
}
