namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Application.Contracts.HardwareDiagnostics;
    using Contracts.Client;

    /// <summary>
    ///     Handles the <see cref="HardwareDiagnosticTestFinishedEvent" />
    /// </summary>
    public class HardwareDiagnosticTestFinishedConsumer : Consumes<HardwareDiagnosticTestFinishedEvent>
    {
        private readonly ISasNoteAcceptorProvider _sasNoteAcceptorProvider;

        /// <summary>
        ///     Creates a HardwareDiagnosticTestFinishedConsumer instance
        /// </summary>
        /// <param name="sasNoteAcceptorProvider">An instance of <see cref="ISasNoteAcceptorProvider" /></param>
        public HardwareDiagnosticTestFinishedConsumer(ISasNoteAcceptorProvider sasNoteAcceptorProvider)
        {
            _sasNoteAcceptorProvider = sasNoteAcceptorProvider ??
                                       throw new ArgumentNullException(nameof(sasNoteAcceptorProvider));
        }

        /// <inheritdoc />
        public override void Consume(HardwareDiagnosticTestFinishedEvent theEvent)
        {
            if (theEvent.DeviceCategory == HardwareDiagnosticDeviceCategory.NoteAcceptor)
            {
                _sasNoteAcceptorProvider.DiagnosticTestActive = false;
            }
        }
    }
}