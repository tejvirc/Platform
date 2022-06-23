namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class ActionTouch : IRobotService, IDisposable
    {
        private readonly Configuration _config;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly ILog _logger;
        private readonly IEventBus _eventBus;
        private Automation _automator;
        private Timer _ActionTouchTimer;
        private bool _disposed;

        public ActionTouch(Configuration config, ILobbyStateManager lobbyStateManager, ILog logger, Automation automator, IEventBus eventBus)
        {
            _config = config;
            _automator = automator;
            _lobbyStateManager = lobbyStateManager;
            _logger = logger;
            _eventBus = eventBus;
        }

        ~ActionTouch()
        {
            Dispose(false);
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
            }

            _disposed = true;
        }

        public string Name => typeof(ActionTouch).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(ActionTouch) };

        public void Execute()
        {
            _ActionTouchTimer = new Timer(
                                (sender) =>
                                {
                                   if(_lobbyStateManager.CurrentState is LobbyState.Game)
                                    {
                                        BalanceCheck();
                                        PerformAction();
                                    }
                                },
                                null,
                                _config.Active.IntervalTouch,
                                Timeout.Infinite);
        }

        private void BalanceCheck()
        {
            _eventBus.Publish(new BalanceCheckEvent());
        }

        private void PerformAction()
        {
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


        public void Halt()
        {
            _ActionTouchTimer.Dispose();
        }

        public void Initialize()
        {
        }
    }
}
