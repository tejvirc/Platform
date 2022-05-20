namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Printer
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a printer status.</summary>
    [Serializable]
    public class PrinterStatus : GdsSerializableMessage, IEquatable<PrinterStatus>, ITransactionSource
    {
        /// <summary>Constructor</summary>
        public PrinterStatus() : base(GdsConstants.ReportId.PrinterStatus) { }

        /// <summary>Gets or sets the identifier of the transaction.</summary>
        /// <value>The identifier of the transaction.</value>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets the reserved 1.</summary>
        /// <value>The reserved 1.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the paper in chute.</summary>
        /// <value>True if paper in chute, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool PaperInChute
        { get; set; }

        /// <summary>Gets or sets a value indicating whether the paper empty.</summary>
        /// <value>True if paper empty, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool PaperEmpty { get; set; }

        /// <summary>Gets or sets a value indicating whether the paper low.</summary>
        /// <value>True if paper low, false if not.</value>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool PaperLow { get; set; }

        /// <summary>Gets or sets a value indicating whether the paper jam.</summary>
        /// <value>True if paper jam, false if not.</value>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool PaperJam { get; set; }

        /// <summary>Gets or sets a value indicating whether the top of form.</summary>
        /// <value>True if top of form, false if not.</value>
        [FieldOrder(6)]
        [FieldBitLength(1)]
        public bool TopOfForm
        { get; set; }

        /// <summary>Gets or sets the print head open.</summary>
        /// <value>The print head open.</value>
        [FieldOrder(7)]
        [FieldBitLength(1)]
        public bool PrintHeadOpen { get; set; }

        /// <summary>Gets or sets a value indicating whether the chassis open.</summary>
        /// <value>True if chassis open, false if not.</value>
        [FieldOrder(8)]
        [FieldBitLength(1)]
        public bool ChassisOpen { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [TransactionId={TransactionId}, PaperInChute={PaperInChute}, PaperEmpty={PaperEmpty}, PaperLow={PaperLow}, PaperJam={PaperJam}, TopOfForm={TopOfForm}, PrintHeadOpen={PrintHeadOpen}, ChassisOpen={ChassisOpen}]");
        }

        /// <inheritdoc/>
        public bool Equals(PrinterStatus other)
        {
            if (other == null)
                return false;

            return PaperLow == other.PaperLow &&
                   PaperEmpty == other.PaperEmpty &&
                   PaperInChute == other.PaperInChute &&
                   PaperJam == other.PaperJam &&
                   PrintHeadOpen == other.PrintHeadOpen &&
                   TopOfForm == other.TopOfForm &&
                   ChassisOpen == other.ChassisOpen;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as PrinterStatus);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 1499833;
            hashCode *= 19 + PaperLow.GetHashCode();
            hashCode *= 19 + PaperEmpty.GetHashCode();
            hashCode *= 19 + PaperInChute.GetHashCode();
            hashCode *= 19 + PaperJam.GetHashCode();
            hashCode *= 19 + PrintHeadOpen.GetHashCode();
            hashCode *= 19 + TopOfForm.GetHashCode();
            hashCode *= 19 + ChassisOpen.GetHashCode();
            return hashCode;
        }
    }
}