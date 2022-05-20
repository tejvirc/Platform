namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;

    /// <summary>
    ///     The set lamp command
    /// </summary>
    [Serializable]
    public class SetLamps : GdsSerializableMessage, IEquatable<SetLamps>, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="SetLamps"/>
        /// </summary>
        public SetLamps()
            : base(GdsConstants.ReportId.ReelControllerSetLights)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event </summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary> Gets or sets number of lamps being set</summary>
        [FieldOrder(1)]
        public byte LampCount { get; set; }

        /// <summary> Gets or sets lamp data</summary>
        [FieldOrder(2)]
        [FieldCount(nameof(LampCount))]
        public GdsReelLampData[] ReelLampData { get; set; }

        /// <inheritdoc />
        public bool Equals(SetLamps other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                    TransactionId == other.TransactionId &&
                    LampCount == other.LampCount &&
                    ReelLampData == other.ReelLampData);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) ||
                    obj.GetType() == GetType() &&
                    Equals((SetLamps)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)LampCount;
                hashCode = (hashCode * 397) ^ TransactionId.GetHashCode();
                hashCode = (hashCode * 397) ^ ReelLampData.GetHashCode();
                return hashCode;
            }
        }
    }
}