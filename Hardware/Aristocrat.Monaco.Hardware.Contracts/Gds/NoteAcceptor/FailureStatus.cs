namespace Aristocrat.Monaco.Hardware.Contracts.Gds.NoteAcceptor
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a failure status.</summary>
    [Serializable]
    public class FailureStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public FailureStatus() : base(GdsConstants.ReportId.NoteAcceptorFailureStatus) { }

        /// <summary>Gets or sets a value indicating whether the diagnostic code.</summary>
        /// <value>True if diagnostic code, false if not.</value>
        [FieldOrder(0)]
        [FieldBitLength(1)]
        public bool DiagnosticCode { get; set; }

        /// <summary>Gets or sets the reserved 1.</summary>
        /// <value>The reserved 1.</value>
        [FieldOrder(1)]
        [FieldBitLength(2)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the non volatile memory is OK.</summary>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool NvmError { get; set; }

        /// <summary>Gets or sets a value indicating whether the component error.</summary>
        /// <value>True if component error, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool ComponentError { get; set; }

        /// <summary>Gets or sets a value indicating whether the optical components are OK.</summary>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool OpticalError { get; set; }

        /// <summary>Gets or sets a value indicating whether the mechanical components are OK.</summary>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool MechanicalError { get; set; }

        /// <summary>Gets or sets a value indicating whether the firmware is OK.</summary>
        [FieldOrder(6)]
        [FieldBitLength(1)]
        public bool FirmwareError { get; set; }

        /// <summary>Gets or sets the error code.</summary>
        /// <value>The error code.</value>
        [FieldOrder(7)]
        public byte ErrorCode { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [FirmwareError : {FirmwareError}, MechanicalError : {MechanicalError}, OpticalError : {OpticalError}, ComponentError : {ComponentError}, NvmError : {NvmError}, DiagnosticCode : {DiagnosticCode} ErrorCode : {ErrorCode}]");
        }
    }
}