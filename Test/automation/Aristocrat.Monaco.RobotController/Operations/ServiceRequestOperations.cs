namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ServiceRequestOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly LobbyStateChecker _stateChecker;
        private readonly RobotLogger _logger;
        private readonly Automation _automator;
        private readonly RobotController _robotController;
        private Timer _serviceRequestTimer;
        private Timer _handlerTimer;
        private bool _disposed;

        public ServiceRequestOperations(IEventBus eventBus, RobotLogger logger, Automation automator, LobbyStateChecker sc, RobotController robotController)
        {
            _stateChecker = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
        }

        ~ServiceRequestOperations() => Dispose(false);

        public void Reset()
        {
            _disposed = false;
        }

        public void Execute()
        {
            _logger.Info("ServiceRequestOperations Has Been Initiated!", GetType().Name);
            SubscribeToEvents();
            _serviceRequestTimer = new Timer(
                               (sender) =>
                               {
                                   RequestService();
                               },
                               null,
                               _robotController.Config.ActiveGameMode.IntervalServiceRequest,
                               _robotController.Config.ActiveGameMode.IntervalServiceRequest);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<CallAttendantButtonOnEvent>(
                this,
                _ =>
                {
                    Task.Delay(Constants.ServiceRequestDelayDuration).ContinueWith(
                    _ =>
                    {
                        _eventBus.Publish(new CallAttendantButtonOffEvent());
                    });
                });
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _automator.ServiceButton(false);
            Dispose();
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
        }

        private bool IsValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _stateChecker.IsLobbyStateGame && !_stateChecker.IsGameLoading;
        }
    }
}
