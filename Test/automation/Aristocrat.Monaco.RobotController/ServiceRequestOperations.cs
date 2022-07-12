namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class ServiceRequestOperations : IRobotOperations
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

        ~ServiceRequestOperations() => Dispose(false);

        public void Reset()
        {
            _disposed = true;
        }

        public void Execute()
        {
            _logger.Info("ServiceRequestOperations Has Been Initiated!", GetType().Name);
            _serviceRequestTimer = new Timer(
                               (sender) =>
                               {
                                   RequestService();
                               },
                               null,
                               _robotController.Config.Active.IntervalServiceRequest,
                               _robotController.Config.Active.IntervalServiceRequest);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _automator.ServiceButton(false);
            _serviceRequestTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_handlerTimer is not null)
                {
                    _handlerTimer.Dispose();
                }
                _handlerTimer = null;
                if (_serviceRequestTimer is not null)
                {
                    _serviceRequestTimer.Dispose();
                }
                _serviceRequestTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
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
                (sender) =>
                {
                    _automator.ServiceButton(false);
                    _handlerTimer.Dispose();
                },
                null,
                Constants.ServiceRequestDelayDuration,
                System.Threading.Timeout.Infinite);
        }

        private bool IsValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return isBlocked && _sc.IsGame && !_sc.IsGameLoading;
        }
    }
}
