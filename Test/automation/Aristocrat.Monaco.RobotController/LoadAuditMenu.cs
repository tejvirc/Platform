namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Threading;
    using Vgt.Client12.Application.OperatorMenu;

    internal class LoadAuditMenu : IRobotOperations, IDisposable
    {
        private readonly Configuration _config;
        private readonly IOperatorMenuLauncher _launcher;
        private readonly Automation _automator;
        private readonly ILog _logger;
        private readonly IEventBus _eventBus;
        private Timer _loadAuditMenuTimer;
        private bool _disposed;

        public LoadAuditMenu(Configuration config, IOperatorMenuLauncher launcher, Automation automator, ILog logger, IEventBus eventBus)
        {
            _config = config;
            _launcher = launcher;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            SubscribeToEvents();
        }

        ~LoadAuditMenu() => Dispose(false);

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<LoadAuditMenuEvent>(this, HandleEvent);
        }

        private void HandleEvent(LoadAuditMenuEvent obj)
        {
            _logger.Info($"Loading the audit menu.");
            _automator.LoadAuditMenu();
        }

        public void Execute()
        {
            _loadAuditMenuTimer = new Timer(
                (sender) =>
                {
                    if (Validate())
                    {
                        _eventBus.Publish(new LoadAuditMenuEvent());
                    }
                },
                null,
                _config.Active.IntervalLoadAuditMenu,
                Timeout.Infinite);
        }

        private bool Validate() => !_launcher.IsOperatorKeyDisabled;

        public void Halt() => _loadAuditMenuTimer?.Dispose();

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Halt();
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
