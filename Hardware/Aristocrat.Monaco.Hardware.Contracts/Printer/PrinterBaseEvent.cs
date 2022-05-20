namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using Properties;
    using System;
    using Kernel;
    using static System.FormattableString;

    /// <summary>Definition of the PrinterBaseEvent class.</summary>
    /// <remarks>All other printer events are derived from this event.</remarks>
    [Serializable]
    public class PrinterBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterBaseEvent"/> class.
        /// </summary>
        protected PrinterBaseEvent()
        {
            PrinterId = 1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterBaseEvent"/> class.
        /// </summary>
        /// <param name="printerId">The ID of the printer associated with the event.</param>
        protected PrinterBaseEvent(int printerId)
        {
            PrinterId = printerId;
        }

        /// <summary>
        ///     Gets the ID of the printer associated with the event.
        /// </summary>
        public int PrinterId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{Resources.PrinterText} {GetType().Name}");
        }
    }
}