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
        private readonly RobotLogger _logger;
        private readonly StateChecker _sc;
        private Timer _loadAuditMenuTimer;
        private Timer _exitAuditMenuTimer;
        private bool _disposed;
        
        public AuditMenuOperations(IEventBus eventBus, RobotLogger logger, Automation automator, Configuration config, StateChecker sc)
        {
            _config = config;
            _sc = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
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
                _logger.Error("Requesting Audit Menu - Failed", GetType().Name);
                return;
            }
            _logger.Info("Requesting Audit Menu", GetType().Name);
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
            _logger.Info("Halt Request is Received!", GetType().Name);
            _automator.ExitAuditMenu();
            _loadAuditMenuTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<AuditMenuRequestEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(
                this,
                _ =>
                {
                    _logger.Info("Operator Menu Entered Succeeded!", GetType().Name);
                });

            _eventBus.Subscribe<OperatorMenuExitedEvent>(
                this,
                _ =>
                {
                    _logger.Info("Operator Menu Exited Successfully!", GetType().Name);
                });
        }

        private void HandleEvent(AuditMenuRequestEvent obj)
        {
            RequestAuditMenu();
        }

        private bool IsValid() =>  (_sc.IsChooser || _sc.IsGame);

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
