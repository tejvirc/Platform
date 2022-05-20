namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;

    /// <summary> Definition of the ErrorWhilePrintingEvent class.</summary>
    /// <remarks>
    ///     This event is posted if an error has occurred while printing.
    ///     It is typically sent by the Printer Service when it detects that an error happened while
    ///     printing was in progress and consumed by the Printer Monitor to display the appropriate
    ///     message on the screen, if supported by the game.
    /// </remarks>
    [Serializable]
    public class ErrorWhilePrintingEvent : PrinterBaseEvent
    {
        /// <summary>Initializes a new instance of the <see cref="ErrorWhilePrintingEvent" /> class.</summary>
        public ErrorWhilePrintingEvent()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ErrorWhilePrintingEvent" /> class with the printer's ID.</summary>
        /// <param name="printerId">The associated printer's ID.</param>
        public ErrorWhilePrintingEvent(int printerId)
            : base(printerId)
        {
        }
    }
}