namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;

    internal sealed class ServiceRequestOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly StateChecker _sc;
        private readonly RobotLogger _logger;
        private readonly Automation _automator;
        private readonly RobotController _robotController;
        private Timer _serviceRequestTimer;
        private Timer _handlerTimer;
        private bool _disposed;

        public ServiceRequestOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, RobotController robotController)
        {
            _sc = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
        }


        public void Reset()
        {
        }

        public void Execute()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ServiceRequestOperations));
            }

            _logger.Info("ServiceRequestOperations Has Been Initiated!", GetType().Name);
            _serviceRequestTimer = new Timer(
                               _ => RequestService(),
                               null,
                               _robotController.Config.Active.IntervalServiceRequest,
                               _robotController.Config.Active.IntervalServiceRequest);
        }

        public void Halt()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ServiceRequestOperations));
            }

            _logger.Info("Halt Request is Received!", GetType().Name);
            _eventBus.UnsubscribeAll(this);
            _serviceRequestTimer?.Halt();
            _automator.ServiceButton(false);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_handlerTimer != null)
            {
                _handlerTimer.Dispose();
                _handlerTimer = null;
            }

            if (_serviceRequestTimer != null)
            {
                _serviceRequestTimer.Dispose();
                _serviceRequestTimer = null;
            }

            _eventBus.UnsubscribeAll(this);
            _disposed = true;
        }

        private void RequestService()
        {
            if (!IsValid())
            {
                return;
            }

            _logger.Info("RequestService Received!", GetType().Name);
            _automator.ServiceButton(true);
            _handlerTimer = new Timer(
                _ =>
                {
                    _automator.ServiceButton(false);
                    _handlerTimer.Dispose();
                },
                null,
                Constants.ServiceRequestDelayDuration,
                Timeout.Infinite);
        }

        private bool IsValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _sc.IsGame && !_sc.IsGameLoading;
        }
    }
}
