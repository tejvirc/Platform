namespace Aristocrat.Monaco.Mgam.Services.CreditValidators
{
    using System;
    using Accounting.Contracts;
    using Aristocrat.Mgam.Client.Logging;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Used to control access to the player bank.
    /// </summary>
    public class CashOutHandler : ICashOut, IDisposable
    {
        private readonly ILogger<CashOutHandler> _logger;
        private readonly IPlayerBank _bank;
        private readonly IEventBus _eventBus;
        private readonly object _cashOutLock = new object();
        private bool _active;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ICashOut" /> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="bank"><see cref="IPlayerBank" />.</param>
        /// <param name="eventBus"><see cref="IEventBus" />.</param>
        public CashOutHandler(
            ILogger<CashOutHandler> logger,
            IPlayerBank bank,
            IEventBus eventBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<TransferOutCompletedEvent>(this, evt => Deactivate());
            _eventBus.Subscribe<TransferOutFailedEvent>(this, evt => Deactivate());
        }

        /// <inheritdoc />
        public long Balance => _bank.Balance;

        /// <inheritdoc />
        public long Credits => _bank.Credits;

        /// <inheritdoc />
        public void CashOut()
        {
            lock (_cashOutLock)
            {
                _logger.LogInfo("CashOut");

                if (_active)
                {
                    return;
                }
                
                if (_bank.Balance > 0)
                {
                    _logger.LogInfo("CashOut Active");

                    _active = true;

                    _bank.CashOut();
                }
            }
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void Deactivate()
        {
            lock (_cashOutLock)
            {
                _logger.LogInfo("Deactivate");

                _active = false;
            }
        }
    }
}