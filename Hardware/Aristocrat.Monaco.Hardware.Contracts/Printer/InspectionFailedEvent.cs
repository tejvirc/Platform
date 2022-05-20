namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using Properties;
    using System;

    /// <summary>Definition of the InspectionFailedEvent class.</summary>
    /// <remarks>
    ///     The Inspection Failed Event is posted by the Printer Service when:
    ///     1. An attempt to open the Communication port failed or
    ///     2. A timeout occurred in the Inspecting state.
    /// </remarks>
    [Serializable]
    public class InspectionFailedEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionFailedEvent" /> class.
        /// </summary>
        public InspectionFailedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionFailedEvent" /> class.Initializes a new instance of the
        ///     InspectionFailedEvent class with the printer's ID.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        public InspectionFailedEvent(int printerId)
            : base(printerId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.PrinterText} {Resources.InspectionFailedText}";
        }
    }
}
