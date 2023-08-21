namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;

    /// <summary>Additional information for fault events.</summary>
    /// <seealso cref="T:System.EventArgs"/>
    public class WarningEventArgs : EventArgs
    {
        /// <summary>Gets or sets the warning.</summary>
        /// <value>The warning.</value>
        public PrinterWarningTypes Warning { get; set; }
    }
}
