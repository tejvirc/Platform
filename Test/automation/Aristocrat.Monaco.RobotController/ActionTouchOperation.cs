namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class ActionTouchOperation : IRobotOperations, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly Automation _automator;
        private readonly ILog _logger;
        private readonly StateChecker _sc;
        private Timer _ActionTouchTimer;
        private bool _disposed;
        private static ActionTouchOperation instance = null;
        private static readonly object padlock = new object();
        public static ActionTouchOperation Instantiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new ActionTouchOperation(robotInfo);
                }
                return instance;
            }
        }
        private ActionTouchOperation(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _automator = robotInfo.Automator;
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
        }
        ~ActionTouchOperation() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _ActionTouchTimer = new Timer(
                                (sender) =>
                                {
                                    if (!IsValid()) { return; }
                                    _eventBus.Publish(new ActionTouchEvent());

                                },
                                null,
                                _config.Active.IntervalTouch,
                                _config.Active.IntervalTouch);
        }
        public void Halt()
        {
            _ActionTouchTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ActionTouchEvent>(this, HandleEvent);
        }
        private void HandleEvent(ActionTouchEvent obj)
        {
            if (!IsValid()) { return; }
            var Rng = new Random((int)DateTime.Now.Ticks);
            TouchGameScreen(Rng);
            TouchVbd(Rng);
            if (_config.CurrentGameProfile != null)
            {
                // touch any auxiliary main screen areas configured
                TouchAnyAuxiliaryMainScreenAreas(Rng);
                // touch any auxiliary VBD areas configured
                TouchAnyAuxiliaryVbdAreas(Rng);
            }
        }
        private bool IsValid()
        {
            return _sc.IsGame;
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
                _ActionTouchTimer?.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
        private void TouchAnyAuxiliaryVbdAreas(Random Rng)
        {
            int x, y;
            foreach (var tb in _config.CurrentGameProfile.ExtraVbdTouchAreas)
            {
                try
                {
                    x = Rng.Next(tb.TopLeftX, tb.BottomRightX);
                    y = Rng.Next(tb.TopLeftY, tb.BottomRightY);
                    if (CheckDeadZones(_config.CurrentGameProfile.VbdTouchDeadZones, x, y))
                    {
                        _automator.TouchVBD(x, y);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Info("Error while using Extra Touch VBD Areas: " + ex);
                }
            }
        }
        private void TouchAnyAuxiliaryMainScreenAreas(Random Rng)
        {
            int x, y;
            foreach (var tb in _config.CurrentGameProfile.ExtraMainTouchAreas)
            {
                try
                {
                    x = Rng.Next(tb.TopLeftX, tb.BottomRightX);
                    y = Rng.Next(tb.TopLeftY, tb.BottomRightY);
                    if (CheckDeadZones(_config.CurrentGameProfile.MainTouchDeadZones, x, y))
                    {
                        _automator.TouchMainScreen(x, y);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Info("Error while using Extra Touch Areas: " + ex);
                }
            }
        }
        private void TouchVbd(Random Rng)
        {
            int x, y;
            x = Rng.Next(_config.VirtualButtonDeck.Width);
            y = Rng.Next(_config.VirtualButtonDeck.Height);
            if (CheckDeadZones(_config.CurrentGameProfile.VbdTouchDeadZones, x, y))
            {
                _automator.TouchVBD(x, y);
            }
        }
        private void TouchGameScreen(Random Rng)
        {
            var x = Rng.Next(_config.GameScreen.Width);
            var y = Rng.Next(_config.GameScreen.Height);
            if (CheckDeadZones(_config.CurrentGameProfile.MainTouchDeadZones, x, y))
            {
                _automator.TouchMainScreen(x, y);
            }
        }
        private bool CheckDeadZones(List<TouchBoxes> deadZones, int x, int y)
        {
            foreach (TouchBoxes tb in _config.CurrentGameProfile.MainTouchDeadZones)
            {
                if (x >= tb.TopLeftX && x <= tb.BottomRightX && y >= tb.TopLeftY && y <= tb.BottomRightY)
                {
                    _logger.Info($"NOT touching ({x}, {y}) as it is in a deadzone");
                    return false;
                }
            }

            return true;
        }
    }
}
