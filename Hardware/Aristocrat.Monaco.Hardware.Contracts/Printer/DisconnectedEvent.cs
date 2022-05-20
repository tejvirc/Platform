namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;
    using Properties;

    /// <summary>Definition of the ID Reader DisconnectedEvent class.</summary>
    /// <remarks>This event is posted when ID Reader is disconnected from the USB.</remarks>
    [Serializable]
    public class DisconnectedEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisconnectedEvent" /> class.
        /// </summary>
        public DisconnectedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisconnectedEvent" /> class with the printer's ID.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        public DisconnectedEvent(int printerId)
            : base(printerId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.PrinterText} {Resources.OfflineText}";
        }
    }
}