namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;
    using Hardware.Contracts.NoteAcceptor;
    using Services.DropMode;
    using BillAcceptorMeterReport = Commands.BillAcceptorMeterReport;

    /// <summary>
    ///     Handles the Note Acceptor <see cref="HardwareFaultClearEvent" /> event.
    /// </summary>
    public class NoteAcceptorHardwareFaultClearConsumer : Consumes<HardwareFaultClearEvent>
    {
        private readonly ILogger _logger;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IDropMode _dropMode;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorHardwareFaultClearConsumer" /> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger" />.</param>
        /// <param name="commandFactory"><see cref="ICommandHandlerFactory" />.</param>
        /// <param name="dropMode"><see cref="IDropMode"/>.</param>
        public NoteAcceptorHardwareFaultClearConsumer(
            ILogger<NoteAcceptorHardwareFaultClearConsumer> logger,
            ICommandHandlerFactory commandFactory,
            IDropMode dropMode)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _dropMode = dropMode ?? throw new ArgumentNullException(nameof(dropMode));
        }

        /// <inheritdoc />
        public override async Task Consume(HardwareFaultClearEvent theEvent, CancellationToken cancellationToken)
        {
            if (_dropMode.Active && theEvent.Fault.HasFlag(NoteAcceptorFaultTypes.StackerDisconnected))
            {
                try
                {
                    _logger.LogInfo("NoteAcceptor StackerDisconnected causes BillAcceptorMeterReport");
                    await _commandFactory.Execute(new BillAcceptorMeterReport());
                }
                catch (ServerResponseException ex)
                {
                    _logger.LogError(ex,
                        "NoteAcceptor StackerDisconnected BillAcceptorMeterReport failed ServerResponseException");
                }
            }
        }
    }
}