namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;
    using Contracts.Reel;

    /// <summary>
    ///     The set offsets for the reels.
    /// </summary>
    [Serializable]
    public class SetOffsets : GdsSerializableMessage, IEquatable<SetOffsets>, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="SetOffsets"/>
        /// </summary>
        public SetOffsets()
            : base(GdsConstants.ReportId.ReelControllerSetOffsets)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event </summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary> Gets or sets reel offset data</summary>
        [FieldOrder(1)]
        [FieldCount(ReelConstants.MaxSupportedReels)]
        public int[] ReelOffsets { get; set; }

        /// <inheritdoc />
        public bool Equals(SetOffsets other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                    TransactionId == other.TransactionId &&
                    ReelOffsets == other.ReelOffsets);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((SetOffsets)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ReelOffsets.GetHashCode();
                hashCode = (hashCode * 397) ^ TransactionId.GetHashCode();
                return hashCode;
            }
        }
    }
}
