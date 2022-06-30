namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Threading;

    internal class AuditMenuOperations : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly ILog _logger;
        private readonly StateChecker _sc;
        private Timer _loadAuditMenuTimer;
        private Timer _exitAuditMenuTimer;
        private bool _disposed;
        private static AuditMenuOperations instance = null;
        private static readonly object padlock = new object();
        public static AuditMenuOperations Instantiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new AuditMenuOperations(robotInfo);
                }
                return instance;
            }
        }
        public AuditMenuOperations(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _automator = robotInfo.Automator;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
        }

        ~AuditMenuOperations() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            if (_config.Active.IntervalLoadAuditMenu == 0) { return; }
            _loadAuditMenuTimer = new Timer(
                (sender) =>
                {
                    RequestAuditMenu();
                },
                null,
                _config.Active.IntervalLoadAuditMenu,
                _config.Active.IntervalLoadAuditMenu);
        }

        private void RequestAuditMenu()
        {
            if (!IsValid())
            {
                //Todo: Log Something
                return;
            }
            _logger.Info("Requesting Audit Menu");
            _automator.LoadAuditMenu();
            _exitAuditMenuTimer = new Timer(
                (sender) =>
                {
                    _automator.ExitAuditMenu();
                    _eventBus.Publish(new GameLoadRequestEvent());
                    _exitAuditMenuTimer.Dispose();
                },
                null,
                Constants.AuditMenuDuration,
                Timeout.Infinite);
        }

        public void Halt()
        {
            _loadAuditMenuTimer?.Dispose();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<AuditMenuRequestEvent>(this, HandleEvent);
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

        private void HandleEvent(AuditMenuRequestEvent obj)
        {
            RequestAuditMenu();
        }

        private bool IsValid() => !_sc.IsInRecovery && (_sc.IsChooser || _sc.IsGame);

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _loadAuditMenuTimer?.Dispose();
                _exitAuditMenuTimer?.Dispose();
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
