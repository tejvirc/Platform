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
                                   RequestHardReboot();
                               },
                               null,
                               _config.Active.IntervalRebootMachine,
                               _config.Active.IntervalRebootMachine);

            _SoftRebootTimer = new Timer(
                               (sender) =>
                               {
                                   RequestSoftReboot();
                               },
                               null,
                               _config.Active.IntervalSoftReboot,
                               _config.Active.IntervalSoftReboot);
        }

        private void RequestSoftReboot()
        {
            if (!IsSoftRebootValid()) { return; }
            _eventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
        }

        private void RequestHardReboot()
        {
            if (!IsRebootValid()) { return; }
            OSManager.ResetComputer();
        }

        private bool IsSoftRebootValid()
        {
            return _sc.IsChooser || _sc.IsGame;
        }

        private bool IsRebootValid()
        {
            return _sc.IsInRecovery && (_sc.IsChooser || _sc.IsGame);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<RebootRequestedEvent>(this, HandleEvent);
            _eventBus.Subscribe<SoftRebootRequestedEvent>(this, HandleEvent);
        }

        private void HandleEvent(SoftRebootRequestedEvent obj)
        {
            RequestSoftReboot();
        }

        private void HandleEvent(RebootRequestedEvent obj)
        {
            RequestHardReboot();
        }

        public void Halt()
        {
            _RebootTimer?.Dispose();
            _SoftRebootTimer?.Dispose();
        }
    }
}
