namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using log4net;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    internal class CashoutOperation : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly ILog _logger;
        private readonly StateChecker _sc;
        private static CashoutOperation _instance = null;
        private Timer _actionCashoutTimer;
        private bool _disposed;
        private static readonly object padlock = new object();
        public static CashoutOperation Instantiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (_instance is null)
                {
                    _instance = new CashoutOperation(robotInfo);
                }
                return _instance;
            }
        }
        private CashoutOperation(RobotInfo robotInfo)
        {
            _eventBus = robotInfo.EventBus;
            _config = robotInfo.Config;
            _logger = robotInfo.Logger;
            _sc = robotInfo.StateChecker;
        }
        ~CashoutOperation() => Dispose(false);
        private void HandleEvent(RequestCashoutEvent obj)
        {
            RequestCashOut();
        }
        private bool IsValid()
        {
            return _sc.IsChooser || _sc.IsIdle || _sc.IsPresentationIdle || _sc.IsGame; 
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
            _eventBus.Subscribe<RequestCashoutEvent>(this, HandleEvent);
            _eventBus.Subscribe<TransferOutCompletedEvent>(this, HandleEvent);
        }
        private void HandleEvent(TransferOutCompletedEvent obj)
        {
            RequestBalance();
        }
        public void Execute()
        {
            SubscribeToEvents();
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
            _logger.Info("Requesting Cashout");
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
            _actionCashoutTimer?.Dispose();
        }
    }
}
