namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a failure status.</summary>
    [Serializable]
    public class FailureStatus : GdsSerializableMessage, IEquatable<FailureStatus>
    {
        /// <summary>Constructor</summary>
        public FailureStatus()
            : base(GdsConstants.ReportId.ReelControllerFailureStatus)
        {
        }

        /// <summary>Gets or sets the reel id for the status</summary>
        [FieldOrder(0)]
        [FieldBitLength(8)]
        public int ReelId { get; set; }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(1)]
        [FieldBitLength(2)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the component error.</summary>
        /// <value>True if component error, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool ComponentError { get; set; }

        /// <summary>Gets or sets a value indicating whether the mechanical components are OK.</summary>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool MechanicalError { get; set; }

        /// <summary>Gets or sets a value indicating whether the firmware error.</summary>
        /// <value>True if firmware error, false if not.</value>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool FirmwareError { get; set; }

        /// <summary>Gets or sets a value indicating whether the diagnostic code.</summary>
        /// <value>True if diagnostic code, false if not.</value>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool DiagnosticCode { get; set; }

        /// <summary>Gets or sets a value indicating whether a tamper (out of sync) was detected.</summary>
        /// <value>True if tamper detected, false if not.</value>
        [FieldOrder(6)]
        [FieldBitLength(1)]
        public bool TamperDetected { get; set; }

        /// <summary>Gets or sets a value indicating whether low voltage was detected.</summary>
        /// <value>True if low voltage detected, false if not.</value>
        [FieldOrder(7)]
        [FieldBitLength(1)]
        public bool LowVoltageDetected { get; set; }

        /// <summary>Gets or sets a value indicating whether a communication error was detected.</summary>
        /// <value>True if a communication error was detected, false if not.</value>
        [FieldOrder(8)]
        [FieldBitLength(1)]
        public bool CommunicationError { get; set; }

        /// <summary>Gets or sets a value indicating whether a hardware error was detected.</summary>
        /// <value>True if a hardware error was detected, false if not.</value>
        [FieldOrder(9)]
        [FieldBitLength(1)]
        public bool HardwareError { get; set; }

        /// <summary>Gets or sets a value indicating whether a reel stall was detected.</summary>
        /// <value>True if stall detected, false if not.</value>
        [FieldOrder(10)]
        [FieldBitLength(1)]
        public bool StallDetected { get; set; }

        /// <summary>Gets or sets the error code.</summary>
        /// <value>The error code.</value>
        [FieldOrder(11)]
        [FieldBitLength(5)]
        public byte ErrorCode { get; set; }


        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [ReelId={ReelId}, ComponentError={ComponentError}, MechanicalError={MechanicalError}, FirmwareError={FirmwareError}, DiagnosticCode={DiagnosticCode}, TamperDetected={TamperDetected}, LowVoltageDetected={LowVoltageDetected}, CommunicationError={CommunicationError}, HardwareError={HardwareError}, StallDetected={StallDetected}, ErrorCode={ErrorCode}]");
        }

        /// <inheritdoc />
        public bool Equals(FailureStatus other)
        {
            return !ReferenceEquals(null, other) &&
                   (ReferenceEquals(this, other) ||
                       ReelId == other.ReelId &&
                       Reserved1 == other.Reserved1 &&
                       ComponentError == other.ComponentError &&
                       MechanicalError == other.MechanicalError &&
                       FirmwareError == other.FirmwareError &&
                       DiagnosticCode == other.DiagnosticCode &&
                       TamperDetected == other.TamperDetected &&
                       LowVoltageDetected == other.LowVoltageDetected &&
                       CommunicationError == other.CommunicationError &&
                       HardwareError == other.HardwareError &&
                       StallDetected == other.StallDetected &&
                       ErrorCode == other.ErrorCode);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) ||
                                                   obj.GetType() == GetType() && Equals((FailureStatus)obj));
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ReelId.GetHashCode();
                hashCode = (hashCode * 397) ^ Reserved1.GetHashCode();
                hashCode = (hashCode * 397) ^ ComponentError.GetHashCode();
                hashCode = (hashCode * 397) ^ MechanicalError.GetHashCode();
                hashCode = (hashCode * 397) ^ FirmwareError.GetHashCode();
                hashCode = (hashCode * 397) ^ DiagnosticCode.GetHashCode();
                hashCode = (hashCode * 397) ^ TamperDetected.GetHashCode();
                hashCode = (hashCode * 397) ^ LowVoltageDetected.GetHashCode();
                hashCode = (hashCode * 397) ^ CommunicationError.GetHashCode();
                hashCode = (hashCode * 397) ^ HardwareError.GetHashCode();
                hashCode = (hashCode * 397) ^ StallDetected.GetHashCode();
                hashCode = (hashCode * 397) ^ ErrorCode.GetHashCode();
                return hashCode;
            }
        }
    }
}