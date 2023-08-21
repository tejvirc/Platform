namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;

    public class HandpayReceiptPrintConsumer : Consumes<HandpayReceiptPrintEvent>
    {
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly ILogger<VoucherIssuedConsumer> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayReceiptPrintConsumer"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="commandFactory"><see cref="ICommandHandlerFactory"/>.</param>
        public HandpayReceiptPrintConsumer(
            ILogger<VoucherIssuedConsumer> logger,
            ICommandHandlerFactory commandFactory)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public override async Task Consume(HandpayReceiptPrintEvent theEvent, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(theEvent.Transaction.Barcode))
            {
                await SendVoucherPrinted(theEvent.Transaction.Barcode);
            }
        }

        private async Task SendVoucherPrinted(string barcode)
        {
            try
            {
                await _commandFactory.Execute(new Commands.VoucherPrinted { Barcode = barcode });
            }
            catch (ServerResponseException ex)
            {
                _logger.LogError(ex, "Error sending VoucherPrinted message");
            }
        }
    }
}
