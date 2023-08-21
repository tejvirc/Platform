namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;
    using static System.FormattableString;

    /// <summary>Definition of the FieldOfInterestEvent class.</summary>
    /// <remarks>This event is posted from the PrinterImplementation when a field if interest is printed.</remarks>
    [Serializable]
    public class FieldOfInterestEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldOfInterestEvent" /> class.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        /// <param name="fieldOfInterest">The field of interest printed.</param>
        public FieldOfInterestEvent(int printerId, int fieldOfInterest)
            : base(printerId)
        {
            FieldOfInterest = fieldOfInterest;
        }

        /// <summary>
        ///     Gets the status event for processing.
        /// </summary>
        public int FieldOfInterest { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [FieldOfInterest={FieldOfInterest}]");
        }
    }
}
