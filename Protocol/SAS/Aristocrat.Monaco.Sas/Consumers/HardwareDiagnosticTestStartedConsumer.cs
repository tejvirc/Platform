namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Application.Contracts.HardwareDiagnostics;
    using Contracts.Client;

    /// <summary>
    ///     Handles the <see cref="HardwareDiagnosticTestStartedEvent" />
    /// </summary>
    public class HardwareDiagnosticTestStartedConsumer : Consumes<HardwareDiagnosticTestStartedEvent>
    {
        private readonly ISasNoteAcceptorProvider _sasNoteAcceptorProvider;

        /// <summary>
        ///     Creates a HardwareDiagnosticTestStartedConsumer instance
        /// </summary>
        /// <param name="sasNoteAcceptorProvider">An instance of <see cref="ISasNoteAcceptorProvider" /></param>
        public HardwareDiagnosticTestStartedConsumer(ISasNoteAcceptorProvider sasNoteAcceptorProvider)
        {
            _sasNoteAcceptorProvider = sasNoteAcceptorProvider ??
                                       throw new ArgumentNullException(nameof(sasNoteAcceptorProvider));
        }

        /// <inheritdoc />
        public override void Consume(HardwareDiagnosticTestStartedEvent theEvent)
        {
            if (theEvent.DeviceCategory == HardwareDiagnosticDeviceCategory.NoteAcceptor)
            {
                _sasNoteAcceptorProvider.DiagnosticTestActive = true;
            }
        }
    }
}