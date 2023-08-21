namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;

    /// <summary>Additional information for fault events.</summary>
    /// <seealso cref="T:System.EventArgs"/>
    public class FaultEventArgs : EventArgs
    {
        /// <summary>Gets or sets the type of the fault.</summary>
        /// <value>The type of the fault.</value>
        public PrinterFaultTypes Fault { get; set; }
    }
}
