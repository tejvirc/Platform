namespace Aristocrat.Monaco.Gaming
{
    using Application.Contracts.OperatorMenu;
    using Contracts;
    using Hardware.Contracts.KeySwitch;
    using Hardware.Contracts.Touch;
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     An implementation of <see cref="IGameDiagnostics" />
    /// </summary>
    public class GameDiagnostics : IGameDiagnostics, IDisposable, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly IPropertiesManager _properties;
        private readonly IButtonDeckFilter _buttonDeckFilter;
        private readonly IGameService _gameService;
        private int _selectedGameId;
        private long _denomId;
        private bool _replayCompleted;
        private bool _disposed;
        private int _gameId;
        private string _label;

        private bool _turnOperatorKeyWhenGameExits;
        private bool _waitingForPriorGameExit;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDiagnostics" /> class.
        /// </summary>
        public GameDiagnostics(IEventBus eventBus, IPropertiesManager properties, IOperatorMenuLauncher operatorMenu, IButtonDeckFilter buttonDeckFilter, IGameService gameService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _buttonDeckFilter = buttonDeckFilter ?? throw new ArgumentNullException(nameof(buttonDeckFilter));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));

            _eventBus.Subscribe<GameProcessExitedEvent>(this, Handle);
            _eventBus.Subscribe<OperatorMenuExitingEvent>(this, Handle);
            _eventBus.Subscribe<GameReplayPauseInputEvent>(this, Handle);
            _eventBus.Subscribe<GameReplayCompletedEvent>(this, Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int RelaunchGameId { get; set; }

        public bool AllowInput { get; private set; }

        public IDiagnosticContext Context { get; private set; }

        public bool IsActive { get; private set; }

        public bool AllowResume { get; set; }

        public string Name => nameof(GameDiagnostics);

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(IGameDiagnostics) };

        public void Initialize()
        {
        }

        public void Start(int gameId, long denomId, string label, IDiagnosticContext context, bool allowInput = false)
        {
            Logger.Debug($"Starting diagnostics for gameId {gameId} : denomId {denomId}");

            if (IsActive)
            {
                Logger.Error("Reentry of diagnostics not allowed");
                return;
            }

            IsActive = true;

            // Disable key during diagnostics.
            _operatorMenu.DisableKey(GamingConstants.OperatorMenuDisableKey);
            _eventBus.Publish(new TouchCalibrationDisableEvent());

            _gameId = gameId;
            _denomId = denomId;
            _label = label;
            _selectedGameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            AllowResume = false;
            Context = context;
            AllowInput = allowInput;
            _replayCompleted = false;

            if (_properties.GetValue(GamingConstants.IsGameRunning, false))
            {
                // A game is up, we need to wait until the game exits before starting the diagnostics.
                _waitingForPriorGameExit = true;

                if (_selectedGameId > 0)
                {
                    Logger.Debug($"Relaunch prior game id {_selectedGameId}.");
                    RelaunchGameId = _selectedGameId;
                }
                else
                {
                    RelaunchGameId = 0;
                }

                _gameService.ShutdownBegin();
            }
            else
            {
                Started();
            }
        }

        public void End()
        {
            Logger.Debug("End Diagnostics");

            if (IsActive)
            {
                if(!_gameService.Running)
                {
                    Reset();
                }

                _gameService.ShutdownBegin();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void Handle(GameProcessExitedEvent theEvent)
        {
            if (IsActive && _waitingForPriorGameExit)
            {
                Logger.Debug("Previous game exited.");

                // Game exited because we requested it to shutdown so we can show the replay.
                _waitingForPriorGameExit = false;

                Started();
            }
            else if (IsActive)
            {
                Reset();
            }
        }

        private void Handle(OperatorMenuExitingEvent theEvent)
        {
            Logger.Debug("Operator Menu Is Shutting Down");
            _waitingForPriorGameExit = false;
        }

        private void Handle(GameReplayPauseInputEvent @event)
        {
            if (!_replayCompleted)
            {
                _buttonDeckFilter.FilterMode = ButtonDeckFilterMode.Normal;
            }
        }

        private void Handle(GameReplayCompletedEvent @event)
        {
            _replayCompleted = true;
            _buttonDeckFilter.FilterMode = ButtonDeckFilterMode.Lockup;
        }

        private void Reset()
        {
            _eventBus.Publish(new TouchCalibrationEnableEvent());

            _operatorMenu.EnableKey(GamingConstants.OperatorMenuDisableKey);
            _eventBus.Unsubscribe<OnEvent>(this);

            _gameId = 0;
            _denomId = 0;
            _label = string.Empty;

            if (_selectedGameId > 0)
            {
                _properties.SetProperty(GamingConstants.SelectedGameId, _selectedGameId);
            }

            IsActive = false;
            AllowInput = false;

            _eventBus.Publish(new GameDiagnosticsCompletedEvent(Context));

            if (_turnOperatorKeyWhenGameExits)
            {
                _turnOperatorKeyWhenGameExits = false;
                _operatorMenu.TurnOperatorKey();
            }
            else if (_operatorMenu.IsShowing && !_operatorMenu.Exiting)
            {
                _operatorMenu.Show();
            }
        }

        private void Started()
        {
            _eventBus.Publish(new GameDiagnosticsStartedEvent(_gameId, _denomId, _label, Context));

            // Don't like this, but otherwise the lobby shows before the Operator Menu closes.
            Task.Delay(100).ContinueWith(_ => { _operatorMenu.Hide(); });
        }
    }
}