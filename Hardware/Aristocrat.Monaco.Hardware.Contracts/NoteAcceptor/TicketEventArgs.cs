namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;

    /// <summary>Additional information for ticket validated events.</summary>
    /// <seealso cref="T:System.EventArgs"/>
    public class TicketEventArgs : EventArgs
    {
        /// <summary>Gets or sets the barcode.</summary>
        /// <value>The barcode.</value>
        public string Barcode { get; set; }
    }
}
