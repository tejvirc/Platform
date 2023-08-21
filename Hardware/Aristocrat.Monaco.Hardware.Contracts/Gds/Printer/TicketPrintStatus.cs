namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Printer
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a ticket print status.</summary>
    [Serializable]
    public class TicketPrintStatus : GdsSerializableMessage, IEquatable<TicketPrintStatus>, ITransactionSource
    {
        /// <summary>Constructor</summary>
        public TicketPrintStatus() : base(GdsConstants.ReportId.PrinterTicketPrintStatus) { }

        /// <summary>Gets or sets the identifier of the transaction.</summary>
        /// <value>The identifier of the transaction.</value>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets the reserved 1.</summary>
        /// <value>The reserved 1.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public byte Reserved1 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Aristocrat.Monaco.Hardware.Printer.TicketPrintStatus is ticket
        /// retractable.
        /// </summary>
        /// <value>True if ticket retractable, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool TicketRetractable { get; set; }

        /// <summary>Gets or sets a value indicating whether the print incomplete.</summary>
        /// <value>True if print incomplete, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool PrintIncomplete { get; set; }

        /// <summary>Gets or sets a value indicating whether the print is complete.</summary>
        /// <value>True if print is complete, false if not.</value>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool PrintComplete { get; set; }

        /// <summary>Gets or sets a value indicating whether the field of interest 3.</summary>
        /// <value>True if field of interest 3, false if not.</value>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool FieldOfInterest3 { get; set; }

        /// <summary>Gets or sets a value indicating whether the field of interest 2.</summary>
        /// <value>True if field of interest 2, false if not.</value>
        [FieldOrder(6)]
        [FieldBitLength(1)]
        public bool FieldOfInterest2 { get; set; }

        /// <summary>Gets or sets the field of interest 1.</summary>
        /// <value>The field of interest 1.</value>
        [FieldOrder(7)]
        [FieldBitLength(1)]
        public bool FieldOfInterest1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the print in progress.</summary>
        /// <value>True if print in progress, false if not.</value>
        [FieldOrder(8)]
        [FieldBitLength(1)]
        public bool PrintInProgress { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [TransactionId={TransactionId}, TicketRetractable={TicketRetractable}, PrintIncomplete={PrintIncomplete}, PrintComplete={PrintComplete}, FieldOfInterest1={FieldOfInterest1}, FieldOfInterest2={FieldOfInterest2}, FieldOfInterest3={FieldOfInterest3}, PrintInProgress={PrintInProgress}]");
        }

        /// <inheritdoc/>
        public bool Equals(TicketPrintStatus other)
        {
            if (other == null)
                return false;

            return TicketRetractable == other.TicketRetractable &&
                   PrintIncomplete == other.PrintIncomplete &&
                   PrintComplete == other.PrintComplete &&
                   FieldOfInterest3 == other.FieldOfInterest3 &&
                   FieldOfInterest2 == other.FieldOfInterest2 &&
                   FieldOfInterest1 == other.FieldOfInterest1 &&
                   PrintInProgress == other.PrintInProgress;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as TicketPrintStatus);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 933333398;
            hashCode *= 23003 * TicketRetractable.GetHashCode();
            hashCode *= 23003 * PrintIncomplete.GetHashCode();
            hashCode *= 23003 * PrintComplete.GetHashCode();
            hashCode *= 23003 * FieldOfInterest3.GetHashCode();
            hashCode *= 23003 * FieldOfInterest2.GetHashCode();
            hashCode *= 23003 * FieldOfInterest1.GetHashCode();
            hashCode *= 23003 * PrintInProgress.GetHashCode();
            return hashCode;
        }
    }
}