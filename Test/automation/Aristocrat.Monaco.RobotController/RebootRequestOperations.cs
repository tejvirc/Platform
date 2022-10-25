namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Aristocrat.Monaco.Test.Automation;

    internal sealed class RebootRequestOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly StateChecker _sc;
        private readonly RobotLogger _logger;
        private readonly RobotController _robotController;
        private Timer _rebootTimer;
        private Timer _softRebootTimer;
        private bool _disposed;

        public RebootRequestOperations(IEventBus eventBus, RobotLogger logger, StateChecker sc, RobotController robotController)
        {
            _sc = sc;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
        }


        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_softRebootTimer != null)
            {
                _softRebootTimer.Dispose();
                _softRebootTimer = null;
            }

            if (_rebootTimer != null)
            {
                _rebootTimer.Dispose();
                _rebootTimer = null;
            }

            _eventBus.UnsubscribeAll(this);
            _disposed = true;
        }

        public void Reset()
        {
        }

        public void Execute()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RebootRequestOperations));
            }

            _logger.Info("RebootRequestOperations Has Been Initiated!", GetType().Name);
            _rebootTimer = new Timer(
                               _ => RequestHardReboot(),
                               null,
                               _robotController.Config.Active.IntervalRebootMachine,
                               _robotController.Config.Active.IntervalRebootMachine);

            _softRebootTimer = new Timer(
                               _ => RequestSoftReboot(),
                               null,
                               _robotController.Config.Active.IntervalSoftReboot,
                               _robotController.Config.Active.IntervalSoftReboot);
        }

        public void Halt()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RebootRequestOperations));
            }

            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _rebootTimer?.Halt();
            _softRebootTimer?.Halt();
        }

        private void RequestSoftReboot()
        {
            if (!IsSoftRebootValid())
            {
                return;
            }
            _logger.Info("RequestSoftReboot Received!", GetType().Name);
            _eventBus.Publish(new ExitRequestedEvent(ExitAction.RestartPlatform));
        }

        private void RequestHardReboot()
        {
            if (!IsRebootValid())
            {
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
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _sc.IsInRecovery && (_sc.IsChooser || _sc.IsGame);
        }
    }
}
