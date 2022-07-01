namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using log4net;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    internal class CashoutOperations : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly RobotLogger _logger;
        private readonly StateChecker _sc;
        private Timer _actionCashoutTimer;
        private bool _disposed;
        public CashoutOperations(IEventBus eventBus, Configuration config, RobotLogger logger, StateChecker sc)
        {
            _config = config;
            _sc = sc;
            _logger = logger;
            _eventBus = eventBus;
        }
        ~CashoutOperations() => Dispose(false);
        private void HandleEvent(CashoutRequestEvent obj)
        {
            RequestCashOut();
        }
        private bool IsValid()
        {
            return _sc.IsChooser || (_sc.IsGame && _sc.IsAbleToCashOut); 
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _actionCashoutTimer?.Dispose();
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<CashoutRequestEvent>(this, HandleEvent);
            _eventBus.Subscribe<TransferOutCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                            this,
                            _ =>
                            {
                                ResetTimer();
                            });
        }

        private void ResetTimer()
        {
            _actionCashoutTimer.Change(_config.Active.IntervalCashOut, _config.Active.IntervalCashOut);
        }

        private void HandleEvent(TransferOutCompletedEvent obj)
        {
            RequestBalance();
        }
        public void Execute()
        {
            SubscribeToEvents();
            if (_config.Active.IntervalCashOut == 0) { return; }
            _actionCashoutTimer = new Timer(
                                (sender) =>
                                {
                                    RequestCashOut();
                                },
                                null,
                                _config.Active.IntervalCashOut,
                                _config.Active.IntervalCashOut);
        }
        private void RequestCashOut()
        {
            if (!IsValid()) { return; }
            _logger.Info("Requesting Cashout", GetType().Name);
            _eventBus.Publish(new CashOutButtonPressedEvent());
        }
        private void RequestBalance()
        {
            Task.Run(
                     () =>
                          {
                              Thread.Sleep(5000);
                              _eventBus.Publish(new BalanceCheckEvent());
                          });
        }
        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            RequestCashOut();
            _actionCashoutTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }
    }
}
