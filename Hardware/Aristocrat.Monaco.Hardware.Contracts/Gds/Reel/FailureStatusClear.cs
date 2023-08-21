namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) A failure status clear.</summary>
    [Serializable]
    public class FailureStatusClear : GdsSerializableMessage, IEquatable<FailureStatusClear>
    {
        /// <summary>Constructor</summary>
        public FailureStatusClear()
            : base(GdsConstants.ReportId.ReelControllerFailureStatusClear)
        {
        }

        /// <summary>Gets or sets the reel id for the status</summary>
        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int ReelId { get; set; }

        /// <summary>Gets or sets a value indicating whether a component error should clear.</summary>
        /// <value>True if component error should clear, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool ComponentError { get; set; }

        /// <summary>Gets or sets a value indicating whether a mechanical component error should clear.</summary>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool MechanicalError { get; set; }

        /// <summary>Gets or sets a value indicating whether a firmware error should clear.</summary>
        /// <value>True if firmware error should clear, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool FirmwareError { get; set; }

        /// <summary>Gets or sets a value indicating whether a tamper (out of sync) was detected.</summary>
        /// <value>True if tamper detected, false if not.</value>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool TamperDetected { get; set; }

        /// <summary>Gets or sets a value indicating whether a low voltage should clear.</summary>
        /// <value>True if low voltage detected should clear, false if not.</value>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool LowVoltageDetected { get; set; }

        /// <summary>Gets or sets a value indicating whether a communication error should clear.</summary>
        /// <value>True if a communication error should clear, false if not.</value>
        [FieldOrder(6)]
        [FieldBitLength(1)]
        public bool CommunicationError { get; set; }

        /// <summary>Gets or sets a value indicating whether a hardware error should clear.</summary>
        /// <value>True if a hardware error should clear, false if not.</value>
        [FieldOrder(7)]
        [FieldBitLength(1)]
        public bool HardwareError { get; set; }

        /// <summary>Gets or sets a value indicating whether a stall was detected.</summary>
        /// <value>True if stall detected, false if not.</value>
        [FieldOrder(8)]
        [FieldBitLength(1)]
        public bool StallDetected { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [ComponentError={ComponentError}, MechanicalError={MechanicalError}, FirmwareError={FirmwareError}, TamperDetected={TamperDetected}, LowVoltageDetected={LowVoltageDetected}, CommunicationError={CommunicationError}, HardwareError={HardwareError}, StallDetected={StallDetected}]");
        }

        /// <inheritdoc />
        public bool Equals(FailureStatusClear other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                       ReelId == other.ReelId &&
                       ComponentError == other.ComponentError &&
                       MechanicalError == other.MechanicalError &&
                       FirmwareError == other.FirmwareError &&
                       TamperDetected == other.TamperDetected &&
                       LowVoltageDetected == other.LowVoltageDetected &&
                       CommunicationError == other.CommunicationError &&
                       HardwareError == other.HardwareError &&
                       StallDetected == other.StallDetected);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) ||
                                                   obj.GetType() == GetType() && Equals((FailureStatusClear)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ReelId.GetHashCode();
                hashCode = (hashCode * 397) ^ ComponentError.GetHashCode();
                hashCode = (hashCode * 397) ^ MechanicalError.GetHashCode();
                hashCode = (hashCode * 397) ^ FirmwareError.GetHashCode();
                hashCode = (hashCode * 397) ^ TamperDetected.GetHashCode();
                hashCode = (hashCode * 397) ^ LowVoltageDetected.GetHashCode();
                hashCode = (hashCode * 397) ^ CommunicationError.GetHashCode();
                hashCode = (hashCode * 397) ^ HardwareError.GetHashCode();
                hashCode = (hashCode * 397) ^ StallDetected.GetHashCode();
                return hashCode;
            }
        }
    }
}