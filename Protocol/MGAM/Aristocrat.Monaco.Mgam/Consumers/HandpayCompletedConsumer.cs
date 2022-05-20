namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;
    using Gaming.Contracts;
    using Services.Lockup;
    using Services.Notification;
    using EndSession = Commands.EndSession;

    /// <summary>
    ///     Handles the <see cref="HandpayCompletedEvent" /> event.
    /// </summary>
    public class HandpayCompletedConsumer : Consumes<HandpayCompletedEvent>
    {
        private readonly ILockup _lockup;
        private readonly INotificationLift _notificationLift;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly ILogger<HandpayCompletedConsumer> _logger;
        private readonly IPlayerBank _playerBank;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayCompletedConsumer" /> class.
        /// </summary>
        public HandpayCompletedConsumer(
            ILockup lockup,
            INotificationLift notificationLift,
            ICommandHandlerFactory commandFactory,
            ILogger<HandpayCompletedConsumer> logger,
            IPlayerBank bank)
        {
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playerBank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        /// <inheritdoc />
        public override async Task Consume(HandpayCompletedEvent theEvent, CancellationToken cancellationToken)
        {
            switch (theEvent.Transaction.HandpayType)
            {
                case HandpayType.CancelCredit:
                    await _notificationLift.Notify(NotificationCode.CanceledCreditHandPay);
                    try
                    {
                        if (_playerBank.Balance == 0)
                        {
                            _logger.LogInfo("SendEndSession credits 0.");
                            await _commandFactory.Execute(new EndSession());
                        }
                    }
                    catch (ServerResponseException ex)
                    {
                        _logger.LogError(ex, "SendEndSession failed ServerResponseException");
                    }

                    break;
            }
        }
    }
}