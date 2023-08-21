namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;

    /// <summary>Additional information for fault events.</summary>
    /// <seealso cref="T:System.EventArgs"/>
    public class FaultEventArgs : EventArgs
    {
        /// <summary>Gets or sets the fault.</summary>
        /// <value>The fault.</value>
        public NoteAcceptorFaultTypes Fault { get; set; }
    }
}