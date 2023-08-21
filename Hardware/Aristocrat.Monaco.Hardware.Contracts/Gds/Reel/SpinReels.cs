namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;

    /// <summary>
    ///     The spin reels command
    /// </summary>
    [Serializable]
    public class SpinReels : GdsSerializableMessage, IEquatable<SpinReels>, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="SpinReels"/>
        /// </summary>
        public SpinReels()
            : base(GdsConstants.ReportId.ReelControllerSpinReels)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event</summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary> Gets or sets number of reels being spun</summary>
        [FieldOrder(1)]
        public byte ReelCount { get; set; }

        /// <summary> Gets or sets reel spin data</summary>
        [FieldOrder(2)]
        [FieldCount(nameof(ReelCount))]
        public GdsReelSpinData[] ReelSpinData { get; set; }

        /// <inheritdoc />
        public bool Equals(SpinReels other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                    TransactionId == other.TransactionId &&
                    ReelCount == other.ReelCount &&
                    ReelSpinData == other.ReelSpinData);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((SpinReels)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)ReelCount;
                hashCode = (hashCode * 397) ^ ReelSpinData.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionId.GetHashCode();
                return hashCode;
            }
        }
    }
}
