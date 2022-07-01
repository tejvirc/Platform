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
        private readonly RobotLogger _logger;
        private Timer _RebootTimer;
        private Timer _SoftRebootTimer;
        private bool _disposed;
        public RebootRequestOperations(IEventBus eventBus, RobotLogger logger, Configuration config, StateChecker sc)
        {
            _config = config;
            _sc = sc;
            _logger = logger;
            _eventBus = eventBus;
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
            if (!IsSoftRebootValid())
            {
                _logger.Error("RequestSoftReboot Validation Failed", GetType().Name);
                return;
            }
            _logger.Info("RequestSoftReboot Received!", GetType().Name);
            _eventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
        }

        private void RequestHardReboot()
        {
            if (!IsRebootValid())
            {
                _logger.Error("RequestHardReboot Validation Failed", GetType().Name);
                return;
            }
            _logger.Info("RequestHardReboot Received!", GetType().Name);
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
            _logger.Info("Halt Request is Received!", GetType().Name);
            _RebootTimer?.Dispose();
            _SoftRebootTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }
    }
}
