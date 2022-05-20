namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;

    /// <summary>
    ///     The set brightness of the lights on a reel. Set ReelId to 0 to set all reel lights.
    /// </summary>
    [Serializable]
    public class SetBrightness : GdsSerializableMessage, IEquatable<SetBrightness>, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="SetBrightness"/>
        /// </summary>
        public SetBrightness()
            : base(GdsConstants.ReportId.ReelControllerSetReelBrightness)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event </summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets the reel Id value. Set the value to 0 to set the lights on all reels </summary>
        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int ReelId { get; set; }

        /// <summary>Gets or sets the brightness as a percentage value from 0 to 100 </summary>
        [FieldOrder(2)]
        [FieldBitLength(8)]
        public int Brightness { get; set; }

        /// <inheritdoc />
        public bool Equals(SetBrightness other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) || ReelId == other.ReelId && Brightness == other.Brightness &&
                       TransactionId == other.TransactionId);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((SetBrightness)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ReelId;
                hashCode = (hashCode * 397) ^ ReelId;
                hashCode = (hashCode * 397) ^ Brightness;
                hashCode = (hashCode * 397) ^ TransactionId.GetHashCode();
                return hashCode;
            }
        }
    }
}