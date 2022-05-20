namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;

    /// <summary>
    ///     Consumes <seealso cref="VoucherIssuedEvent"/> event.
    /// </summary>
    public class VoucherIssuedConsumer : Consumes<VoucherIssuedEvent>
    {
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly ILogger<VoucherIssuedConsumer> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherIssuedConsumer"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="commandFactory"><see cref="ICommandHandlerFactory"/>.</param>
        public VoucherIssuedConsumer(
            ICommandHandlerFactory commandFactory,
            ILogger<VoucherIssuedConsumer> logger)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public override async Task Consume(VoucherIssuedEvent theEvent, CancellationToken cancellationToken)
        {
            await SendVoucherPrinted(theEvent.Transaction.Barcode);
        }

        private async Task SendVoucherPrinted(string barcode)
        {
            try
            {
                _logger.LogInfo($"SendVoucherPrinted Barcode: {barcode}");
                await _commandFactory.Execute(new Commands.VoucherPrinted() { Barcode = barcode });
            }
            catch (ServerResponseException ex)
            {
                _logger.LogError(ex, "SendVoucherPrinted failed ServerResponseException");
            }
        }
    }
}
