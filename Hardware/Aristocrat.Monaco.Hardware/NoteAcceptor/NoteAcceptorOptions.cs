namespace Aristocrat.Monaco.Hardware.NoteAcceptor
{
    using Contracts.NoteAcceptor;
    using System;

    public class NoteAcceptorOptions
    {
        /// <summary>Gets or sets the last document result.</summary>
        /// <value>The last document result.</value>
        public DocumentResult LastDocumentResult { get; set; }

        /// <summary>Gets or sets the activation time.</summary>
        /// <value>The last activation time.</value>
        public DateTime ActivationTime { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the device was in the process of stacking when powered up.
        /// </summary>
        public bool WasStackingOnLastPowerUp { get; set; }
    }
}
