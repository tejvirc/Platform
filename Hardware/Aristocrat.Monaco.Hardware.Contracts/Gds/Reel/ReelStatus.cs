namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a reel status.</summary>
    [Serializable]
    public class ReelStatus : GdsSerializableMessage, IEquatable<ReelStatus>
    {
        /// <summary>Constructor</summary>
        public ReelStatus()
            : base(GdsConstants.ReportId.ReelControllerStatus)
        {
        }

        /// <summary>Gets or sets the reserved 1.</summary>
        /// <value>The reserved 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(2)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets the reel id for the status</summary>
        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int ReelId { get; set; }

        /// <summary> Gets or sets whether or not the reel has stalled </summary>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool ReelStall { get; set; }

        /// <summary> Gets or sets whether or not the reel has been tampered with </summary>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool ReelTampered { get; set; }

        /// <summary> Gets or sets whether or not the reel is connected </summary>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool Connected { get; set; }

        /// <summary> Gets or sets whether or not a reel requested to spin/nudge to goal resulted in a request error </summary>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool RequestError { get; set; }

        /// <summary>Gets or sets a value indicating whether or not a reel requested to spin/nudge to goal resulted in low voltage.</summary>
        [FieldOrder(6)]
        [FieldBitLength(1)]
        public bool LowVoltage { get; set; }

        /// <summary>Gets or sets a value indicating whether or not a reel requested to home resulted in an error.</summary>
        [FieldOrder(7)]
        [FieldBitLength(1)]
        public bool FailedHome { get; set; }

        /// <inheritdoc />
        public bool Equals(ReelStatus other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) || Reserved1 == other.Reserved1 && ReelId == other.ReelId &&
                       ReelStall == other.ReelStall && ReelTampered == other.ReelTampered &&
                       Connected == other.Connected && RequestError == other.RequestError &&
                       LowVoltage == other.LowVoltage && FailedHome == other.FailedHome);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((ReelStatus)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Reserved1.GetHashCode();
                hashCode = (hashCode * 397) ^ ReelId;
                hashCode = (hashCode * 397) ^ ReelStall.GetHashCode();
                hashCode = (hashCode * 397) ^ ReelTampered.GetHashCode();
                hashCode = (hashCode * 397) ^ Connected.GetHashCode();
                hashCode = (hashCode * 397) ^ RequestError.GetHashCode();
                hashCode = (hashCode * 397) ^ LowVoltage.GetHashCode();
                hashCode = (hashCode * 397) ^ FailedHome.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [ReelId={ReelId}, ReelStall={ReelStall}, ReelTampered={ReelTampered}, Connected={Connected}, RequestError={RequestError}, LowVoltage={LowVoltage}, FailedHome={FailedHome}]");
        }
    }
}