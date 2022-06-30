namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Threading;

    internal class RebootRequestOperations : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly StateChecker _sc;
        private readonly Automation _automator;
        private readonly ILog _logger;
        private Timer _RebootTimer;
        private Timer _SoftRebootTimer;
        private bool _disposed;
        public RebootRequestOperations(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
            _automator = robotInfo.Automator;
        }
        ~RebootRequestOperations() => Dispose(false);
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
                _eventBus.UnsubscribeAll(this);
                _RebootTimer?.Dispose();
                _SoftRebootTimer?.Dispose();
            }
            _disposed = true;
        }

        public void Execute()
        {
            SubscribeToEvents();
            _RebootTimer = new Timer(
                               (sender) =>
                               {
                                   if (!IsRebootValid()) { return; }
                                   _eventBus.Publish(new RebootRequestedEvent());
                               },
                               null,
                               _config.Active.IntervalRebootMachine,
                               _config.Active.IntervalRebootMachine);

            _SoftRebootTimer = new Timer(
                               (sender) =>
                               {
                                   if (!IsSoftRebootValid()) { return; }
                                   _eventBus.Publish(new SoftRebootRequestedEvent());
                               },
                               null,
                               _config.Active.IntervalSoftReboot,
                               _config.Active.IntervalSoftReboot);
        }

        private bool IsSoftRebootValid()
        {
            return _sc.IsChooser || _sc.IsGame;
        }

        private bool IsRebootValid()
        {
            return _sc.IsChooser || _sc.IsGame;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<RebootRequestedEvent>(this, HandleEvent);
            _eventBus.Subscribe<SoftRebootRequestedEvent>(this, HandleEvent);
        }

        private void HandleEvent(SoftRebootRequestedEvent obj)
        {
            if (!IsSoftRebootValid()) { return; }
            _eventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
        }

        private void HandleEvent(RebootRequestedEvent obj)
        {
            if (!IsRebootValid()) { return; }
            //OSManager.ResetComputer();
        }

        public void Halt()
        {
            _eventBus.UnsubscribeAll(this);
            _RebootTimer?.Dispose();
            _SoftRebootTimer?.Dispose();
        }
    }
}
