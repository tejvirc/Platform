namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Threading;

    internal class AuditMenuOperation : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly ILog _logger;
        private readonly StateChecker _sc;
        private Timer _loadAuditMenuTimer;
        private Timer _exitAuditMenuTimer;
        private bool _disposed;
        private static AuditMenuOperation instance = null;
        private static readonly object padlock = new object();
        public static AuditMenuOperation Instantiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new AuditMenuOperation(robotInfo);
                }
                return instance;
            }
        }
        public AuditMenuOperation(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _automator = robotInfo.Automator;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
        }

        ~AuditMenuOperation() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _loadAuditMenuTimer = new Timer(
                (sender) =>
                {
                    if (!IsValid()) { return; }
                    {
                        _eventBus.Publish(new LoadAuditMenuEvent());
                    }
                },
                null,
                _config.Active.IntervalLoadAuditMenu,
                _config.Active.IntervalLoadAuditMenu);
        }
        public void Halt()
        {
            _loadAuditMenuTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<LoadAuditMenuEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(
                this,
                _ =>
                {
                    //log
                });

            _eventBus.Subscribe<OperatorMenuExitedEvent>(
                this,
                _ =>
                {
                   //log
                });
        }

        private void HandleEvent(LoadAuditMenuEvent obj)
        {
            if (!IsValid())
            {
                //Todo: Log Something
                return;
            }
            _logger.Info("Requesting Audit Menu");
            _automator.EnableCashOut(true);
            _automator.LoadAuditMenu();
            _exitAuditMenuTimer = new Timer(
                (sender) =>
                {
                    _automator.ExitAuditMenu();
                    _eventBus.Publish(new LoadGameEvent());
                    _exitAuditMenuTimer.Dispose();
                },
                null,
                Constants.AuditMenuDuration,
                System.Threading.Timeout.Infinite);
        }

        private bool IsValid() => _sc.IsChooser || _sc.IsGame;

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _loadAuditMenuTimer?.Dispose();
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
