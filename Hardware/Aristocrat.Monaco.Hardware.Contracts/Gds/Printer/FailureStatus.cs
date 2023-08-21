namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Printer
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a failure status.</summary>
    [Serializable]
    public class FailureStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public FailureStatus() : base(GdsConstants.ReportId.PrinterFailureStatus) { }

        /// <summary>Gets or sets a value indicating whether there is a diagnostic code.</summary>
        /// <value>True if a diagnostic code is specified, false if not.</value>
        [FieldOrder(0)]
        [FieldBitLength(1)]
        public bool DiagnosticCode { get; set; }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(1)]
        [FieldBitLength(3)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the temperature error.</summary>
        /// <value>True if temperature error, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool TemperatureError { get; set; }

        /// <summary>Gets or sets a value indicating whether the print head damaged.</summary>
        /// <value>True if print head damaged, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool PrintHeadDamaged { get; set; }

        /// <summary>Gets or sets a value indicating whether the nvm error.</summary>
        /// <value>True if nvm error, false if not.</value>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool NvmError { get; set; }

        /// <summary>Gets or sets a value indicating whether the firmware error.</summary>
        /// <value>True if firmware error, false if not.</value>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool FirmwareError { get; set; }

        /// <summary>Gets or sets the error code.</summary>
        /// <value>The error code.</value>
        [FieldOrder(6)]
        public byte ErrorCode { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [FirmwareError={FirmwareError}, NvmError={NvmError}, PrintHeadDamaged={PrintHeadDamaged}, TemperatureError={TemperatureError}, DiagnosticCode={DiagnosticCode}, ErrorCode={ErrorCode}]");
        }
    }
}