namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using Properties;
    using System;

    /// <summary>Definition of the ID Reader ConnectedEvent class.</summary>
    /// <remarks>This event is posted when ID Reader is connected to the USB.</remarks>
    [Serializable]
    public class ConnectedEvent : PrinterBaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedEvent"/> class.
        /// </summary>
        public ConnectedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedEvent"/> class with the printer's ID.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        public ConnectedEvent(int printerId)
            : base(printerId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.PrinterText} {Resources.OfflineText} {Resources.ClearedText}";
        }
    }
}
