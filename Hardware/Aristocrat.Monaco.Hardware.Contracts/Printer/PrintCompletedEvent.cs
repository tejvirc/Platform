namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;

    /// <summary>
    ///     Definition of the PrintCompletedEvent class.
    ///     <remarks>
    ///         This event is posted by the Printer in response to the
    ///         Printer Implementations posting of a Printer Finished Event.  It indicates that the
    ///         Printer has completed a print job and is ready for another.
    ///     </remarks>
    /// </summary>
    [Serializable]
    public class PrintCompletedEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrintCompletedEvent" /> class.
        /// </summary>
        public PrintCompletedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrintCompletedEvent" /> class.Initializes a new instance of the
        ///     PrintCompletedEvent class with the printer's ID.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        public PrintCompletedEvent(int printerId)
            : base(printerId)
        {
        }

        /// <summary>
        ///     Gets or sets field of interest
        /// </summary>
        public bool FieldOfInterest { get; set; }
    }
}