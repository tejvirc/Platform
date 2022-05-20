namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a failure status.</summary>
    [Serializable]
    public class FailureStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public FailureStatus() : base(GdsConstants.ReportId.CardReaderFailureStatus) { }

        /// <summary>Gets or sets a value indicating whether the diagnostic code.</summary>
        /// <value>True if diagnostic code, false if not.</value>
        [FieldOrder(0)]
        [FieldBitLength(1)]
        public bool DiagnosticCode { get; set; }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(1)]
        [FieldBitLength(5)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the icc power fail.</summary>
        /// <value>True if icc power fail, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool IccPowerFail { get; set; }

        /// <summary>Gets or sets a value indicating whether the firmware error.</summary>
        /// <value>True if firmware error, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool FirmwareError { get; set; }

        /// <summary>Gets or sets the error code.</summary>
        /// <value>The error code.</value>
        [FieldOrder(4)]
        public byte ErrorCode { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [FirmwareError={FirmwareError}, IccPowerFail={IccPowerFail}, DiagnosticCode={DiagnosticCode}, ErrorCode={ErrorCode}]");
        }
    }
}