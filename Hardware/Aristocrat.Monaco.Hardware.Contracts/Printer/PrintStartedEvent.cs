namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;

    /// <summary>Definition of the PrintStartedEvent class.</summary>
    /// <remarks>
    ///     Nearly all of the printing in XSpin is done with Printer Definition Language (PDL).
    ///     The printer implementation allows for non-PDL printing, in which printing starts when
    ///     a FormFeed character is sent to the printer.  FormFeed() will post this event.
    ///     But in PDL printing the user (the Player Terminal) will call the Printer's Print(Ticket).
    ///     This uses the Ticket Content to first "Resolve" the print data, then calls the specific
    ///     printer's (e.g. PrinterIthatca850) PrintRaw() with a render object as a parameter. The Printer's PrintRaw render
    ///     parameter
    ///     will yield a string that can be passed to the specific printer.  At this point,
    ///     PrintRaw posts the PrintStartedEvent and sends the data through the serial port.
    /// </remarks>
    [Serializable]
    public class PrintStartedEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrintStartedEvent" /> class.
        /// </summary>
        public PrintStartedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrintStartedEvent" /> class.Initializes a new instance of the
        ///     PrintStartedEvent class with the printer's ID.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        public PrintStartedEvent(int printerId)
            : base(printerId)
        {
        }
    }
}
