namespace Aristocrat.Monaco.Gaming
{
    using Application.Contracts.Media;
    using Contracts;
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Timers;
    using Hardware.Contracts.Button;
    using Runtime;
    using Hardware.Contracts.Cabinet;
    using Runtime.Client;
    using ButtonState = Runtime.Client.ButtonState;

    /// <summary>
    ///     Implements <see cref="IAttendantService"/> interface
    /// </summary>
    public class AttendantService : IAttendantService, IDisposable
    {
        // Game Button State enumerations
        private enum GameButtonState
        {
            /// <summary>Button state is not set.</summary>
            NotSet = -1,

            /// <summary>Button is disabled.</summary>
            Disabled,

            /// <summary>Button is enabled but not selected.</summary>
            Enabled,

            /// <summary>Button is enabled and selected (i.e. highlighted).</summary>
            Selected
        }

        private const uint ServiceButtonId = (uint)ButtonLogicalId.Service - (uint)ButtonLogicalId.ButtonBase;

        private readonly IEventBus _eventBus;
        private readonly IMediaPlayerResizeManager _resizeManager;
        private readonly IRuntimeFlagHandler _runtimeFlag;
        private readonly IRuntime _runtime;
        //private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly IPropertiesManager _propertiesManager;

        private bool _disposed;
        private bool _isServiceRequested;
        private bool _isGameWaitingForPlayerInput;
        private GameButtonState _serviceButtonState;

        private bool _showConfirmation;
        private bool _attendantServiceTimeoutSupported;
        private Timer _attendantServiceRequestTimeoutTimer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AttendantService"/> class.
        /// </summary>
        /// <param name="eventBus">An instance of <see cref="IEventBus"/></param>
        /// <param name="resizeManager">An instance of <see cref="IMediaPlayerResizeManager"/></param>
        /// <param name="runtimeFlag">An instance of <see cref="IRuntimeFlagHandler"/></param>
        /// <param name="runtime">An instance of <see cref="IRuntime"/></param>
        /// <param name="cabinetDetectionService">An instance of <see cref="ICabinetDetectionService"/></param>
        /// <param name="propertiesManager">An instance of <see cref="IPropertiesManager"/></param>
        public AttendantService(
            IEventBus eventBus,
            IMediaPlayerResizeManager resizeManager,
            IRuntimeFlagHandler runtimeFlag,
            IRuntime runtime,
            //ICabinetDetectionService cabinetDetectionService,
            IPropertiesManager propertiesManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _resizeManager = resizeManager ?? throw new ArgumentNullException(nameof(resizeManager));
            _runtimeFlag = runtimeFlag ?? throw new ArgumentNullException(nameof(runtimeFlag));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            //_cabinetDetectionService = cabinetDetectionService ??
            //                           throw new ArgumentNullException(nameof(cabinetDetectionService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IAttendantService) };

        /// <inheritdoc />
        public bool IsServiceRequested
        {
            get => _isServiceRequested;

            set
            {
                // Don't request service if currently resizing media players
                if (_isServiceRequested == value || _resizeManager.IsResizing)
                {
                    return;
                }

                _isServiceRequested = value;
                _runtimeFlag.SetServiceRequested(_isServiceRequested);
                RaiseCallAttendant();
                UpdateServiceButtonState();

                if (_attendantServiceTimeoutSupported)
                {
                    UpdateAttendantServiceTimer();
                }
            }
        }

        /// <inheritdoc />
        public bool IsMediaContentUsed { get; set; }

        /// <inheritdoc />
        public void Initialize()
        {
            _showConfirmation = /*_cabinetDetectionService.IsTouchVbd()*/ false;
            _serviceButtonState = GameButtonState.NotSet;

            _eventBus.Subscribe<GameInitializationCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<WaitingForPlayerInputStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<WaitingForPlayerInputEndedEvent>(this, HandleEvent);

            _attendantServiceTimeoutSupported = (bool)_propertiesManager.GetProperty(GamingConstants.AttendantServiceTimeoutSupportEnabled, false);
            if (_attendantServiceTimeoutSupported)
            {
                _eventBus.Subscribe<PrimaryGameStartedEvent>(this, HandleEvent);
                SetupAttendantServiceTimer();
            }
        }

        /// <inheritdoc />
        public void OnServiceButtonPressed()
        {
            // Ignore the service button press if currently resizing media players
            if (_resizeManager.IsResizing)
            {
                return;
            }

            if (_showConfirmation && !IsServiceRequested && !IsMediaContentUsed)
            {
                _eventBus.Publish(new ShowServiceConfirmationEvent());
            }
            else
            {
                IsServiceRequested = !IsServiceRequested;
            }
        }

        /// <inheritdoc />
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
                _eventBus.UnsubscribeAll(this);

                // ReSharper disable once UseNullPropagation
                if (_attendantServiceRequestTimeoutTimer != null)
                {
                    _attendantServiceRequestTimeoutTimer.Dispose();
                }
            }

            _disposed = true;
        }

        private void HandleEvent(GameInitializationCompletedEvent evt)
        {
            // Set _serviceButtonState to NotSet every time a new game is launched to enforce publishing SetGameButtonStateEvent
            _serviceButtonState = GameButtonState.NotSet;

            _isGameWaitingForPlayerInput = false;
            UpdateServiceButtonState();
        }

        private void HandleEvent(WaitingForPlayerInputStartedEvent evt)
        {
            _isGameWaitingForPlayerInput = true;
            UpdateServiceButtonState();
        }

        private void HandleEvent(WaitingForPlayerInputEndedEvent evt)
        {
            _isGameWaitingForPlayerInput = false;
            UpdateServiceButtonState();
        }

        private void HandleEvent(PrimaryGameStartedEvent evt)
        {
            if (IsServiceRequested)
            {
                IsServiceRequested = false;
            }
        }

        private void RaiseCallAttendant()
        {
            if (_isServiceRequested)
            {
                _eventBus.Publish(new CallAttendantButtonOnEvent());
            }
            else
            {
                _eventBus.Publish(new CallAttendantButtonOffEvent());
            }
        }

        private void UpdateServiceButtonState()
        {
            // Requirements:
            // If Media Content is used - Service Button should be enabled only when Game is waiting for player input (i.e. not in the middle of reel spinning nor incrementing win meter)
            // If Media Content is not used - Service Button should be enabled all the time.
            var newServiceButtonState = GameButtonState.Disabled;
            if (!IsMediaContentUsed || !_isGameWaitingForPlayerInput)
            {
                newServiceButtonState = IsServiceRequested ? GameButtonState.Selected : GameButtonState.Enabled;
            }

            if (_serviceButtonState != newServiceButtonState)
            {
                _serviceButtonState = newServiceButtonState;
                SetRuntimeServiceButtonState(_serviceButtonState);
            }
        }

        private void SetRuntimeServiceButtonState(GameButtonState serviceButtonState)
        {
            var systemButtonState = serviceButtonState switch
            {
                GameButtonState.Enabled => ButtonState.Enabled,
                GameButtonState.Selected => ButtonState.Enabled | ButtonState.LightOn,
                _ => ButtonState.NotSet
            };

            _runtime.UpdateButtonState(ServiceButtonId, ButtonMask.Enabled | ButtonMask.Lamps, systemButtonState);
        }

        private void SetupAttendantServiceTimer()
        {
            var timeout = _propertiesManager.GetValue(
                GamingConstants.AttendantServiceTimeoutInMilliseconds,
                180000);
            _attendantServiceRequestTimeoutTimer = new Timer(timeout);
            _attendantServiceRequestTimeoutTimer.Elapsed += ClearAttendantServiceRequest;
            _attendantServiceRequestTimeoutTimer.AutoReset = false;
        }

        private void UpdateAttendantServiceTimer()
        {
            if (_isServiceRequested)
            {
                _attendantServiceRequestTimeoutTimer?.Start();
                return;
            }

            _attendantServiceRequestTimeoutTimer?.Stop();
        }

        private void ClearAttendantServiceRequest(object source, ElapsedEventArgs args)
        {
            if (IsServiceRequested)
            {
                IsServiceRequested = false;
            }
        }
    }
}
