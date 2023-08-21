namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;

    /// <summary>Additional information for note validated events.</summary>
    /// <seealso cref="T:System.EventArgs"/>
    public class NoteEventArgs : EventArgs
    {
        /// <summary>Gets or sets the note.</summary>
        /// <value>The note.</value>
        public INote Note { set; get; }
    }
}
