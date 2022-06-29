namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Threading;

    internal class ServiceRequestOperation : IRobotOperations, IDisposable
    {
        private IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly StateChecker _sc;
        private readonly ILog _logger;
        private readonly Automation _automator;
        private Timer _ServiceRequestTimer;
        private Timer _handlerTimer;
        private bool _disposed;

        public ServiceRequestOperation(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
            _automator = robotInfo.Automator;
        }
        ~ServiceRequestOperation() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _ServiceRequestTimer = new Timer(
                               (sender) =>
                               {
                                   if (!IsValid()) { return; }
                                   _eventBus.Publish(new ServiceRequestedEvent());
                               },
                               null,
                               _config.Active.IntervalServiceRequest,
                               _config.Active.IntervalServiceRequest);
        }

        private bool IsValid()
        {
            return _sc.IsGame;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ServiceRequestedEvent>(this, HandleEvent);
        }

        private void HandleEvent(ServiceRequestedEvent obj)
        {
            if (!IsValid()) { return; }
            _automator.ServiceButton(true);
            _handlerTimer = new Timer(
                (sender) =>
                {
                    _automator.ServiceButton(false);
                    _handlerTimer.Dispose();
                },
                null,
                5000,
                System.Threading.Timeout.Infinite);
        }

        public void Halt()
        {
            throw new NotImplementedException();
        }

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
                _handlerTimer?.Dispose();
                _ServiceRequestTimer?.Dispose();
            }
            _disposed = true;
        }
    }
}
