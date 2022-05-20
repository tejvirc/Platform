namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;

    /// <summary>Additional information for field of interest events.</summary>
    /// <seealso cref="T:System.EventArgs"/>
    public class FieldOfInterestEventArgs : EventArgs
    {
        /// <summary>Gets or sets the field of interest.</summary>
        /// <value>The field of interest.</value>
        public int FieldOfInterest { get; set; }
    }
}
