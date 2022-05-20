namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;

    /// <summary>
    ///     The set the speed of the reels.
    /// </summary>
    [Serializable]
    public class SetSpeed : GdsSerializableMessage, IEquatable<SetSpeed>, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="SetSpeed"/>
        /// </summary>
        public SetSpeed()
            : base(GdsConstants.ReportId.ReelControllerSetReelSpeed)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event </summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary> Gets or sets number of reels being set</summary>
        [FieldOrder(1)]
        public byte ReelCount { get; set; }

        /// <summary> Gets or sets reel speed data</summary>
        [FieldOrder(2)]
        [FieldCount(nameof(ReelCount))]
        public GdsReelSpeedData[] ReelSpeedData { get; set; }

        /// <inheritdoc />
        public bool Equals(SetSpeed other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                    TransactionId == other.TransactionId &&
                    ReelCount == other.ReelCount &&
                    ReelSpeedData == other.ReelSpeedData);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((SetSpeed)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)ReelCount;
                hashCode = (hashCode * 397) ^ ReelSpeedData.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionId.GetHashCode();
                return hashCode;
            }
        }
    }
}