namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Linq;
    using Aristocrat.Monaco.Gaming.Commands;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Threading;

    internal class ActionCashout : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly IPropertiesManager _pm;
        private readonly ILog _logger;
        private readonly StateChecker _sc;
        private static ActionCashout _instance = null;
        private Timer _actionCashoutTimer;
        private bool _disposed;
        private static readonly object padlock = new object();

        public static ActionCashout Instantiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (_instance is null)
                {
                    _instance = new ActionCashout(robotInfo);
                }
                return _instance;
            }
        }

        private ActionCashout(RobotInfo robotInfo)
        {
            _eventBus = robotInfo.EventBus;
            _config = robotInfo.Config;
            _automator = robotInfo.Automator;
            _pm = robotInfo.PropertiesManager;
            _logger = robotInfo.Logger;
            _sc = robotInfo.StateChecker;
        }

        ~ActionCashout() => Dispose(false);

        private void HandleEvent(ActionCashoutEvent obj)
        {
            _logger.Info("Requesting Cashout");
            _eventBus.Publish(new CashOutButtonPressedEvent());
        }
        
        private bool IsValid()
        {
            return _sc.IsChooser || _sc.IsIdle || _sc.IsPresentationIdle || _sc.IsGame || _sc.IsDisabled; // CashOutButtonPressedConsumer::Consume -> Cashout unavailable if gameplay state is not idle
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
            }
            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ActionCashoutEvent>(this, HandleEvent);
        }

        public void Execute()
        {
            SubscribeToEvents();
            _actionCashoutTimer = new Timer(
                                (sender) =>
                                {
                                    if (!IsValid()) { return; }
                                    _eventBus.Publish(new ActionCashoutEvent());
                                },
                                null,
                                _config.Active.IntervalCashOut,
                                _config.Active.IntervalCashOut);
        }

        public void Halt()
        {
            _actionCashoutTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }
    }
}
