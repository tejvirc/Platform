namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;

    internal class TouchOperations : IRobotOperations
    {
        private readonly IEventBus _eventBus;
        private readonly Automation _automator;
        private readonly RobotLogger _logger;
        private readonly StateChecker _stateChecker;
        private readonly RobotController _robotController;
        private Timer _actionTouchTimer;
        private bool _disposed;

        public TouchOperations(IEventBus eventBus, RobotLogger logger, Automation automator, StateChecker sc, RobotController robotController)
        {
            _stateChecker = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
            _robotController = robotController;
        }

        ~TouchOperations() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            _disposed = false;
        }

        public void Execute()
        {
            _logger.Info("TouchOperations Has Been Initiated!", GetType().Name);
            _actionTouchTimer = new Timer(
                                (sender) =>
                                {
                                    TouchRequest();
                                },
                                null,
                                _robotController.Config.Active.IntervalTouch,
                                _robotController.Config.Active.IntervalTouch);
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_actionTouchTimer is not null)
                {
                    _logger.Info("_actionTouchTimer is disposing ***", GetType().Name);
                    _actionTouchTimer.Dispose();
                }
                _actionTouchTimer = null;
                _eventBus.UnsubscribeAll(this);
            }
            _disposed = true;
        }

        private void TouchRequest()
        {
            if (!IsValid())
            {
                return;
            }
            _logger.Info("TouchRequest Is Received!", GetType().Name);
            var Rng = new Random((int)DateTime.Now.Ticks);

            TouchGameScreen(Rng);
            TouchVbd(Rng);

            if (_robotController.Config.CurrentGameProfile != null)
            {
                // touch any auxiliary main screen areas configured
                TouchAnyAuxiliaryMainScreenAreas(Rng);
                // touch any auxiliary VBD areas configured
                TouchAnyAuxiliaryVbdAreas(Rng);
            }
        }

        private bool IsValid()
        {
            var isBlocked = _robotController.IsBlockedByOtherOperation(new List<RobotStateAndOperations>());
            return !isBlocked && _stateChecker.IsGame && !_stateChecker.IsGameLoading;
        }

        private void TouchAnyAuxiliaryVbdAreas(Random Rng)
        {
            int x, y;
            foreach (var tb in _robotController.Config.CurrentGameProfile.ExtraVbdTouchAreas)
            {
                try
                {
                    x = Rng.Next(tb.TopLeftX, tb.BottomRightX);
                    y = Rng.Next(tb.TopLeftY, tb.BottomRightY);
                    if (CheckDeadZones(_robotController.Config.CurrentGameProfile.VbdTouchDeadZones, x, y))
                    {
                        _automator.TouchVBD(x, y);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Info("Error while using Extra Touch VBD Areas: " + ex, GetType().Name);
                }
            }
        }

        private void TouchAnyAuxiliaryMainScreenAreas(Random Rng)
        {
            int x, y;
            foreach (var tb in _robotController.Config.CurrentGameProfile.ExtraMainTouchAreas)
            {
                try
                {
                    x = Rng.Next(tb.TopLeftX, tb.BottomRightX);
                    y = Rng.Next(tb.TopLeftY, tb.BottomRightY);
                    if (CheckDeadZones(_robotController.Config.CurrentGameProfile.MainTouchDeadZones, x, y))
                    {
                        _automator.TouchMainScreen(x, y);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Info("Error while using Extra Touch Areas: " + ex, GetType().Name);
                }
            }
        }

        private void TouchVbd(Random Rng)
        {
            int x, y;
            x = Rng.Next(_robotController.Config.VirtualButtonDeck.Width);
            y = Rng.Next(_robotController.Config.VirtualButtonDeck.Height);
            if (CheckDeadZones(_robotController.Config.CurrentGameProfile.VbdTouchDeadZones, x, y))
            {
                _automator.TouchVBD(x, y);
            }
        }

        private void TouchGameScreen(Random Rng)
        {
            var x = Rng.Next(_robotController.Config.GameScreen.Width);
            var y = Rng.Next(_robotController.Config.GameScreen.Height);
            if (CheckDeadZones(_robotController.Config.CurrentGameProfile.MainTouchDeadZones, x, y))
            {
                _automator.TouchMainScreen(x, y);
            }
        }

        private bool CheckDeadZones(List<TouchBoxes> deadZones, int x, int y)
        {
            foreach (TouchBoxes tb in deadZones)
            {
                if (x >= tb.TopLeftX && x <= tb.BottomRightX && y >= tb.TopLeftY && y <= tb.BottomRightY)
                {
                    _logger.Info($"NOT touching ({x}, {y}) as it is in a deadzone", GetType().Name);
                    return false;
                }
            }
            return true;
        }
    }
}
