namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Timers;
    using Accounting.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Stateless;

    /// <summary>
    ///     An <see cref="IDisableConditionSaga" />  implementation
    /// </summary>
    public class DisableConditionSaga : IDisableConditionSaga, IDisposable
    {
        private const int TimeoutInterval = 1000;

        private const PersistenceLevel Level = PersistenceLevel.Critical;
        private const string DeviceClass = @"DeviceClass";
        private const string DeviceIdKey = @"DeviceId";
        private const string MessageKey = @"Message";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IPersistentStorageAccessor _accessor;
        private readonly IBank _bank;

        private readonly ICabinetState _cabinetState;
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IGamePlayState _gamePlayState;
        private readonly IGameHistory _gameHistory;

        private readonly ConcurrentDictionary<IDevice, DeviceLock> _queuedEntries =
            new ConcurrentDictionary<IDevice, DeviceLock>();

        private readonly IEgmStateManager _stateManager;
        private IDevice _device;
        private DeviceLock _deviceLock;
        private bool _disposed;

        private StateMachine<State, Trigger> _state;

        private Timer _timer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisableConditionSaga" /> class.
        /// </summary>
        /// <param name="cabinetState">An <see cref="ICabinetState" /> instance.</param>
        /// <param name="gamePlayState">An <see cref="IGamePlayState" /> instance.</param>
        /// <param name="bank">An <see cref="IBank" /> instance.</param>
        /// <param name="stateManager">An <see cref="IEgmStateManager" /> instance.</param>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="storageManager">An <see cref="IPersistentStorageManager" /> instance.</param>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        /// /// <param name="gameHistory">An <see cref="IGameHistory" /> instance.</param>
        public DisableConditionSaga(
            ICabinetService cabinetState,
            IGamePlayState gamePlayState,
            IBank bank,
            IEgmStateManager stateManager,
            IG2SEgm egm,
            IPersistentStorageManager storageManager,
            IEventBus eventBus,
            IGameHistory gameHistory)
        {
            if (storageManager == null)
            {
                throw new ArgumentNullException(nameof(storageManager));
            }

            _state = new StateMachine<State, Trigger>(State.Start);

            _cabinetState = cabinetState ?? throw new ArgumentNullException(nameof(cabinetState));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));

            _accessor = storageManager.GetAccessor(Level, GetType().ToString());

            _eventBus.Subscribe<GameIdleEvent>(
                this,
                _ =>
                {
                    if (_state.CanFire(Trigger.ZeroCredits) && _bank.QueryBalance() == 0)
                    {
                        _state.Fire(Trigger.ZeroCredits);
                    }
                    else if (_state.CanFire(Trigger.GameIdle))
                    {
                        _state.Fire(Trigger.GameIdle);
                    }
                });

            _eventBus.Subscribe<CabinetIdleEvent>(
                this,
                _ =>
                {
                    if (_state.CanFire(Trigger.EgmIdle))
                    {
                        _state.Fire(Trigger.EgmIdle);
                    }
                });

            _eventBus.Subscribe<BankBalanceChangedEvent>(
                this,
                evt =>
                {
                    if (_state.CanFire(Trigger.ZeroCredits) && !_gamePlayState.InGameRound && evt.NewBalance == 0 && !_gameHistory.IsRecoveryNeeded)
                    {
                        _state.Fire(Trigger.ZeroCredits);
                    }
                });

            _timer = new Timer(TimeoutInterval);
            _timer.Elapsed += TimeoutTimerElapsed;
            _timer.Enabled = false;
        }

        private bool Pending
            =>
                _state.IsInState(State.WaitingForGameIdle) || _state.IsInState(State.WaitingForZeroCredits) ||
                _state.IsInState(State.WaitingForEgmIdle) || _state.IsInState(State.AcquiringLock);

        /// <inheritdoc />
        public DateTime Disabled { get; private set; }

        /// <inheritdoc />
        public void Enter(
            IDevice device,
            DisableCondition condition,
            TimeSpan timeToLive,
            Func<string> message,
            Action<bool> onDisable,
            EgmState state = EgmState.HostLocked)
        {
            InternalEnter(device, condition, timeToLive, message, false, onDisable, state);
        }

        /// <inheritdoc />
        public void Enter(
            IDevice device,
            DisableCondition condition,
            TimeSpan timeToLive,
            Func<string> message,
            bool persist,
            Action<bool> onDisable)
        {
            InternalEnter(device, condition, timeToLive, message, persist, onDisable, EgmState.HostLocked);
        }

        /// <inheritdoc />
        public void Exit(IDevice device, DisableCondition condition, TimeSpan timeToLive, Action<bool> onEnable)
        {
            Logger.Debug($"Attempting to exit configuration mode for device {device}");

            if (onEnable == null)
            {
                throw new ArgumentNullException(nameof(onEnable));
            }

            if (_deviceLock == null)
            {
                Logger.Debug("Attempting to exit without a lock");
                return;
            }

            if (_queuedEntries.TryRemove(device, out var deviceLock))
            {
                Logger.Debug($"Device {device} was queued - exiting");
                _stateManager.Enable(deviceLock.DisableKey, device, deviceLock.DisableState);
                RespondAsync(onEnable, true);
                return;
            }

            if (device != _device)
            {
                Logger.Warn(
                    $"Attempted to exit configuration mode for device {device} while device {_device} has the lock");
                RespondAsync(onEnable, false);
                return;
            }

            // If we don't have a lock we can bail immediately
            if (!Enabled(device))
            {
                Logger.Debug($"Device {device} doesn't have a lock - exiting");
                RespondAsync(onEnable, true);
                return;
            }

            // If we're trying to acquire a lock we need to bail on that request
            if (Pending)
            {
                Logger.Debug($"Device {device} has a pending lock - cancelling");
                _state.Fire(Trigger.Expired);
                RespondAsync(onEnable, true);
                return;
            }

            _deviceLock.OnEnable = onEnable;

            if (condition != DisableCondition.Idle || !_queuedEntries.IsEmpty || HasConfigurationPeriodLapsed())
            {
                Logger.Debug($"Exiting configuration mode for device {device}");

                Exchange();
            }
            else
            {
                TimeSpan delay;

                var timeRemaining = DateTime.UtcNow - Disabled;

                if (timeToLive >= GetConfigurationDelayPeriod())
                {
                    delay = GetConfigurationDelayPeriod();
                }
                else
                {
                    delay = timeRemaining < timeToLive ? timeRemaining : timeToLive;
                }

                Task.Delay(delay)
                    .ContinueWith(
                        _ =>
                        {
                            if (HasConfigurationPeriodLapsed())
                            {
                                Logger.Debug($"Exiting configuration mode for device {device}");

                                if (_state.CanFire(Trigger.Enabled))
                                {
                                    _state.Fire(Trigger.Enabled);
                                }
                            }
                            else
                            {
                                Logger.Info($"Exiting configuration mode expired for device {device}");

                                RespondAsync(onEnable, false);
                            }
                        });
            }
        }

        /// <inheritdoc />
        public bool Enabled(IDevice device)
        {
            return (device == _device || _queuedEntries.ContainsKey(device)) && _state.IsInState(State.Locked);
        }

        /// <inheritdoc />
        public void Reenter()
        {
            var deviceId = (int)_accessor[DeviceIdKey];
            if (deviceId == 0)
            {
                return;
            }

            var deviceClass = (string)_accessor[DeviceClass];

            _device = _egm.GetDevice(deviceClass.TrimmedDeviceClass(), deviceId);
            var message = (string)_accessor[MessageKey];
            _deviceLock = new DeviceLock { Message = () => message };

            InitializeStateMachine(State.AcquiringLock);
            OnAcquireLock();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _timer.Dispose();
            }

            _timer = null;

            _disposed = true;
        }

        private void InternalEnter(
            IDevice device,
            DisableCondition condition,
            TimeSpan timeToLive,
            Func<string> message,
            bool persist,
            Action<bool> onDisable,
            EgmState state)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (onDisable == null)
            {
                throw new ArgumentNullException(nameof(onDisable));
            }

            if (state == EgmState.Enabled)
            {
                throw new ArgumentException(@"The Enabled state cannot be specified.", nameof(state));
            }

            Logger.Debug($"Attempting to enter configuration mode for the {device} device ");

            if (_state.IsInState(State.Locked) && _device == device)
            {
                // Update the lock
                _stateManager.Disable(_deviceLock.DisableKey, _device, state, false, message);
                Logger.Info(
                    $"{device} already has the device locked with key {_deviceLock.DisableKey}.  The disable state will be updated to reflect message: {message}");
                RespondAsync(onDisable, true);
                return;
            }

            if (_state.IsInState(State.Locked) || Pending && _device != device)
            {
                AcquireConcurrentLock(device, condition, timeToLive, message, persist, onDisable, state);
                return;
            }

            _deviceLock = new DeviceLock(
                condition,
                message,
                persist,
                onDisable,
                timeToLive == TimeSpan.Zero || timeToLive == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.UtcNow + timeToLive,
                Guid.Empty,
                false,
                state);

            InitializeForDevice(device);
        }

        private void RespondAsync(Action<bool> action, bool result)
        {
            if (action != null && !(_deviceLock?.Notified ?? false))
            {
                Task.Run(() => action(result));
            }
        }

        private void InitializeStateMachine(State initialState = State.Start)
        {
            _state = new StateMachine<State, Trigger>(initialState);

            _state.Configure(State.Start)
                .OnEntry(() => { })
                .Permit(Trigger.Waiting, State.WaitingForGameIdle);

            _state.Configure(State.WaitingForGameIdle)
                .OnEntry(OnWaitForGameIdle)
                .Permit(Trigger.Expired, State.Exited)
                .PermitDynamic(
                    Trigger.GameIdle,
                    () =>
                    {
                        switch (_deviceLock?.DisableCondition)
                        {
                            case DisableCondition.Idle:
                                return State.WaitingForEgmIdle;
                            case DisableCondition.ZeroCredits:
                                return State.WaitingForZeroCredits;
                            case DisableCondition.Immediate:
                                return State.AcquiringLock;
                            default:
                                return State.Error;
                        }
                    });

            _state.Configure(State.WaitingForEgmIdle)
                .OnEntry(OnWaitForEgmIdle)
                .Permit(Trigger.Expired, State.Exited)
                .Permit(Trigger.EgmIdle, State.AcquiringLock);

            _state.Configure(State.WaitingForZeroCredits)
                .OnEntry(OnWaitForZeroCredits)
                .Permit(Trigger.Expired, State.Exited)
                .Permit(Trigger.ZeroCredits, State.AcquiringLock);

            _state.Configure(State.AcquiringLock)
                .OnEntry(OnAcquireLock)
                .Permit(Trigger.Expired, State.Exited)
                .Permit(Trigger.Disabled, State.Locked);

            _state.Configure(State.Locked)
                .OnEntry(OnLocked)
                .OnExit(OnUnlock)
                .Permit(Trigger.Enabled, State.Exited);

            _state.Configure(State.Exited)
                .OnEntryFrom(Trigger.Expired, _ => RespondAsync(_deviceLock.OnDisable, false))
                .OnEntry(OnComplete);

            _state.Configure(State.Error)
                .OnEntry(OnError);

            _state.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error(
                        $"Invalid Disable State Transition. State : {state} Trigger : {trigger}");
                });

            _state.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"Transitioned From : {transition.Source} To : {transition.Destination} with Trigger : {transition.Trigger} for device {_device}");
                });
        }

        private void OnWaitForGameIdle()
        {
            if (_gamePlayState.Idle && !_gameHistory.IsRecoveryNeeded)
            {
                _state.Fire(Trigger.GameIdle);
            }
            else
            {
                Logger.Debug("Game is not idle. Configuration mode is waiting for the GameIdleEvent");
            }
        }

        private void OnWaitForEgmIdle()
        {
            if (_cabinetState.Idle || _stateManager.HasLock)
            {
                _state.Fire(Trigger.EgmIdle);
            }
            else
            {
                Logger.Debug("Cabinet is not idle. Configuration mode is waiting for the CabinetIdleEvent");
            }
        }

        private void OnWaitForZeroCredits()
        {
            if (_bank.QueryBalance() == 0)
            {
                _state.Fire(Trigger.ZeroCredits);
            }
            else
            {
                Logger.Debug("Balance is not zero. Configuration mode is waiting for the credits to reach zero");
            }
        }

        private void OnAcquireLock()
        {
            // Per the protocol this can expire, but we're not going to do that. If we got this far we're done
            _timer.Enabled = false;

            _deviceLock.DisableKey = _stateManager.Disable(_device, _deviceLock.DisableState, false, _deviceLock.Message);
            _state.Fire(Trigger.Disabled);

            Logger.Debug(
                $"Configuration mode acquired the lock for device {_device} with key {_deviceLock.DisableKey}");
        }

        private void OnLocked()
        {
            Disabled = DateTime.UtcNow;

            SaveCurrentState();

            RespondAsync(_deviceLock.OnDisable, true);
        }

        private void OnUnlock()
        {
            _stateManager.Enable(_deviceLock.DisableKey, _device, _deviceLock.DisableState);

            Logger.Debug(
                $"Configuration mode released the lock for device {_device} with key {_deviceLock.DisableKey}");

            RespondAsync(_deviceLock.OnEnable, true);
        }

        private void OnComplete()
        {
            _timer.Enabled = false;

            if (!Exchange())
            {
                _device = null;
                _deviceLock = null;

                SaveCurrentState();
            }
        }

        private void OnError()
        {
            _timer.Enabled = false;
        }

        private bool HasConfigurationPeriodLapsed()
        {
            return DateTime.UtcNow - Disabled > GetConfigurationDelayPeriod();
        }

        private TimeSpan GetConfigurationDelayPeriod()
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();

            return TimeSpan.FromMilliseconds(cabinet.ConfigDelayPeriod);
        }

        private void TimeoutTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (e.SignalTime.ToUniversalTime() > _deviceLock.Timeout && _state.CanFire(Trigger.Expired))
            {
                _state.Fire(Trigger.Expired);
            }
        }

        private void AcquireConcurrentLock(
            IDevice device,
            DisableCondition condition,
            TimeSpan timeToLive,
            Func<string> message,
            bool persist,
            Action<bool> onDisable,
            EgmState state)
        {
            var disableKey = Guid.Empty;
            var notified = false;

            if (Pending)
            {
                Logger.Info($"{_device} is already trying to lock the device.  The enter request has been queued");

                Task.Delay(timeToLive).ContinueWith(
                    _ =>
                    {
                        if (!_queuedEntries.TryRemove(device, out var entry) || entry.DisableKey != Guid.Empty)
                        {
                            return;
                        }

                        Logger.Debug($"Configuration mode expired for device {device} on a queued item");

                        RespondAsync(entry.OnDisable, false);
                    });
            }
            else
            {
                Logger.Info(
                    $"{_device} already has the device locked.  The disable state will be updated to include {message} for device ({device})");

                disableKey = _stateManager.Disable(device, state, false, message);

                RespondAsync(onDisable, true);

                notified = true;
            }

            _queuedEntries.TryAdd(
                device,
                new DeviceLock(
                    condition,
                    message,
                    persist,
                    onDisable,
                    DateTime.UtcNow + timeToLive,
                    disableKey,
                    notified,
                    state));
        }

        private bool Exchange()
        {
            var last = _queuedEntries.LastOrDefault();
            var device = last.Key;
            if (device == null || !_queuedEntries.TryRemove(device, out var entry))
            {
                if (_state.CanFire(Trigger.Enabled))
                {
                    _state.Fire(Trigger.Enabled);
                }
            }
            else
            {
                Logger.Debug($"Pulled an item from the pending queue for device {device}");

                _stateManager.Enable(_deviceLock.DisableKey, _device, _deviceLock.DisableState);

                RespondAsync(_deviceLock.OnEnable, true);

                _deviceLock = entry;

                SaveCurrentState();

                InitializeForDevice(device);

                return true;
            }

            return false;
        }

        private void SaveCurrentState()
        {
            using (var transaction = _accessor.StartTransaction())
            {
                var persist = _deviceLock?.Persist ?? false;
                transaction[DeviceClass] = persist ? _device.DeviceClass : string.Empty;
                transaction[DeviceIdKey] = persist ? _device.Id : 0;
                transaction[MessageKey] = persist ? _deviceLock.Message?.Invoke() : string.Empty;

                transaction.Commit();
            }
        }

        private void InitializeForDevice(IDevice device)
        {
            _device = device;

            InitializeStateMachine();

            Logger.Debug($"Triggering the wait condition for the {device} device ");

            _state.Fire(Trigger.Waiting);

            _timer.Enabled = true;
        }

        private enum State
        {
            Start,
            WaitingForGameIdle,
            WaitingForEgmIdle,
            WaitingForZeroCredits,
            AcquiringLock,
            Locked,
            Exited,
            Error
        }

        private enum Trigger
        {
            Waiting,
            GameIdle,
            EgmIdle,
            ZeroCredits,
            Expired,
            Disabled,
            Enabled
        }

        private class DeviceLock
        {
            public DeviceLock()
            {
                DisableState = EgmState.HostLocked;
                Persist = true;
            }

            public DeviceLock(
                DisableCondition disableCondition,
                Func<string> message,
                bool persist,
                Action<bool> onDisable,
                DateTime timeout,
                Guid disableKey,
                bool notified,
                EgmState state)
            {
                DisableCondition = disableCondition;
                Message = message;
                Persist = persist;
                OnDisable = onDisable;
                Timeout = timeout;
                DisableKey = disableKey;
                Notified = notified;
                DisableState = state;
            }

            public Guid DisableKey { get; set; }

            public DisableCondition DisableCondition { get; }

            public Func<string> Message { get; set; }

            public bool Persist { get; }

            public Action<bool> OnDisable { get; }

            public DateTime Timeout { get; }

            public Action<bool> OnEnable { get; set; }

            public bool Notified { get; }

            public EgmState DisableState { get; }
        }
    }
}
