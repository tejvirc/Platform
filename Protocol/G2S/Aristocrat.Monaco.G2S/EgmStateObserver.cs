namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Reflection;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Gaming.Contracts;
    using Handlers;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     An implementation of an <see cref="IEgmStateObserver" />
    /// </summary>
    public class EgmStateObserver : IEgmStateObserver, IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPlayerBank _bank;
        private readonly IEventBus _bus;
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;

        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IGameHistory _gameHistory;
        private readonly IGamePlayState _gamePlay;
        private readonly ITransactionCoordinator _transactionCoordinator;
        private readonly IPropertiesManager _propertiesManager;

        private bool _initialized;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EgmStateObserver" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="gamePlay">An <see cref="IGamePlayState" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        /// <param name="bank">An <see cref="IPlayerBank" /> instance.</param>
        /// <param name="gameHistory">An <see cref="IGameHistory" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        /// <param name="transactionCoordinator">An <see cref="ITransactionCoordinator" /> instance.</param>
        /// <param name="propertiesManager">An <see cref="IPropertiesManager" /> instance.</param>
        public EgmStateObserver(
            IG2SEgm egm,
            IEventBus eventBus,
            IGamePlayState gamePlay,
            IEventLift eventLift,
            IPlayerBank bank,
            IGameHistory gameHistory,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            ITransactionCoordinator transactionCoordinator,
            IPropertiesManager propertiesManager)
        {
            _transactionCoordinator = transactionCoordinator;
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _bus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            _bus.Subscribe<GameIdleEvent>(this, Handle);
            _bus.Subscribe<PersistentStorageClearStartedEvent>(this, evt => _initialized = false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Subscribe()
        {
            _initialized = true;

            var cabinet = _egm.GetDevice<ICabinetDevice>();

            if (cabinet != null)
            {
                if (!cabinet.Evaluate())
                {
                    CashoutOnDisable(cabinet.Device, cabinet.State, cabinet.FaultId);
                }
            }
        }

        public void Unsubscribe()
        {
            _initialized = false;
        }

        /// <inheritdoc />
        public void NotifyEnabledChanged(IDevice device, bool enabled)
        {
            GenerateEvent(enabled ? EventCode.G2S_CBE002 : EventCode.G2S_CBE001);
        }

        /// <inheritdoc />
        public void NotifyStateChanged(IDevice device, EgmState state, int faultId)
        {
            Logger.Info($"G2S Egm State changed: {device?.DeviceClass} - {state}");

            string eventCode;

            switch (state)
            {
                case EgmState.Enabled:
                    eventCode = EventCode.G2S_CBE205;
                    break;
                case EgmState.OperatorMode:
                    eventCode = EventCode.G2S_CBE206;
                    break;
                case EgmState.AuditMode:
                    eventCode = EventCode.G2S_CBE208;
                    break;
                case EgmState.OperatorDisabled:
                    eventCode = EventCode.G2S_CBE202;
                    break;
                case EgmState.OperatorLocked:
                    eventCode = EventCode.G2S_CBE209;
                    break;
                case EgmState.TransportDisabled:
                    eventCode = EventCode.G2S_CBE201;
                    break;
                case EgmState.HostDisabled:
                    // We may want to do this for all devices (with their applicable event code).  Just need to make it generic
                    if (device is ICabinetDevice)
                    {
                        GenerateEvent(EventCode.G2S_CBE003);
                    }

                    eventCode = EventCode.G2S_CBE204;
                    break;
                case EgmState.EgmDisabled:
                    eventCode = EventCode.G2S_CBE203;
                    break;
                case EgmState.EgmLocked:
                    eventCode = EventCode.G2S_CBE210;
                    break;
                case EgmState.HostLocked:
                    eventCode = EventCode.G2S_CBE211;
                    break;
                case EgmState.DemoMode:
                    eventCode = EventCode.G2S_CBE207;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            GenerateEvent(eventCode);

            _bus.Publish(new EgmStateChangedEvent(state));

            CashoutOnDisable(device, state, faultId);
        }

        /// <inheritdoc />
        public void StateAdded(IDevice device, EgmState state, int faultId)
        {
            Logger.Debug($"G2S Egm state added: {device?.DeviceClass} - {state} with Fault Id({faultId})");

            if ((!_gamePlay.Idle || _gameHistory.IsRecoveryNeeded) && HandledState(state, faultId))
            {
                if (ShouldStartDisableCountdownTimerEvent(device, faultId))
                {
                    _bus.Publish(new DisableCountdownTimerEvent(true));
                }

                return;
            }

            var cabinet = _egm.GetDevice<ICabinetDevice>();

            cabinet?.Evaluate();
        }

        /// <inheritdoc />
        public void StateRemoved(IDevice device, EgmState state, int faultId)
        {
            Logger.Debug($"G2S Egm state removed: {device?.DeviceClass} - {state} - with Fault Id({faultId})");

            var cabinet = _egm.GetDevice<ICabinetDevice>();
            cabinet?.Evaluate();

            if (!_gamePlay.Idle && HandledState(state, faultId))
            {
                if (ShouldStopDisableCountdownTimerEvent(device, faultId, cabinet))
                {
                    _bus.Publish(new DisableCountdownTimerEvent(false));
                }

                return;
            }

            if (cabinet != null)
            {
                CashoutOnDisable(cabinet.Device, cabinet.State, cabinet.FaultId);
            }
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
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private static bool HandledState(EgmState state, int faultId)
        {
            return state == EgmState.EgmDisabled && faultId < 0 ||
                   state == EgmState.TransportDisabled ||
                   state == EgmState.OperatorLocked ||
                   state == EgmState.HostDisabled ||
                   state == EgmState.HostLocked;
        }

        private void GenerateEvent(string eventCode)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (cabinet == null)
            {
                return;
            }

            var status = new cabinetStatus();

            _commandBuilder.Build(cabinet, status);

            _eventLift.Report(cabinet, eventCode, cabinet.DeviceList(status));
        }

        private void Handle(GameIdleEvent data)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (cabinet == null)
            {
                return;
            }

            // We skipped these above, so if the state doesn't change and we're in this state we need to force the notification
            if (!cabinet.Evaluate() && HandledState(cabinet.State, cabinet.FaultId))
            {
                NotifyStateChanged(cabinet.Device, cabinet.State, cabinet.FaultId);
            }
        }

        private void CashoutOnDisable(IDevice device, EgmState state, int faultId)
        {
            // We're going to skip cashout if the host disables the printer.  May need to include other devices as needed
            if (!_initialized || device is IPrinterDevice)
            {
                return;
            }

            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (cabinet != null && !cabinet.MoneyOutEnabled)
            {
                Logger.Debug("Cashout on disable skipped - Money out is disabled");
                return;
            }

            // The faultId will be less than zero for states where the egm is disabled due to a non-hardware based tilt like operating hours (see EgmLock)
            if (HandledState(state, faultId))
            {
                if (!_gameHistory.IsRecoveryNeeded)
                {
                    // TODO: Check cabinet configuration when supported since it can drive the behavior (cash out, no action, etc.)

                    if (_transactionCoordinator.IsTransactionActive)
                    {
                        // This is solely to handle the very rare case that the cabinet was disabled while in a game round and the player was over the jurisdictional credit limit at the end of the game
                        //  They may have been cashed out for partial amount requiring us to cash out the remainder here...
                        _bus.Subscribe<TransactionCompletedEvent>(
                            this,
                            _ =>
                            {
                                _bus.Unsubscribe<TransactionCompletedEvent>(this);

                                if (!(_egm.GetDevice<ICabinetDevice>()?.IsEnabled() ?? true))
                                {
                                    Cashout(state);
                                }
                            });
                    }
                    else if (cabinet == null || !cabinet.HasCondition((d, s, f) => s == EgmState.EgmDisabled && f > 0))
                    {
                        Cashout(state);
                    }
                }
                else
                {
                    Logger.Debug("Cashout on disable skipped due to recovery needed/in-progress");
                }
            }
        }

        private void Cashout(EgmState state)
        {
            if (_bank.Balance <= 0)
            {
                return;
            }

            if (!_bank.CashOut())
            {
                Logger.Error("Cashout on Disable Player bank cashout failed");
            }

            Logger.Debug($"Cashout on disable invoked due to state {state}");
        }

        private bool ShouldStartDisableCountdownTimerEvent(IDevice device, int faultId)
        {
            return DoesDeviceAllowForDisableCountdownEvent(device) &&
                   faultId != (int)CabinetFaults.HardMeterDisabled &&
                   !_propertiesManager.GetValue(GamingConstants.AutocompleteExpired, false) &&
                   !_propertiesManager.GetValue(GamingConstants.AutocompleteSet, false);
        }

        private bool ShouldStopDisableCountdownTimerEvent(IDevice device, int faultId, ICabinetDevice cabinet)
        {
            return DoesDeviceAllowForDisableCountdownEvent(device) &&
                   faultId != (int)CabinetFaults.HardMeterDisabled &&
                   !(cabinet?.HasCondition(
                       (d, s, f) =>
                           s == EgmState.HostLocked || s == EgmState.HostDisabled ||
                           s == EgmState.EgmDisabled && f < 0 && !(d is IPrinterDevice)) ?? false);
        }

        private bool DoesDeviceAllowForDisableCountdownEvent(IDevice device)
        {
            return device != null &&
                   device.RequiredForPlay &&
                   !(device is IPrinterDevice) &&
                   !(device is IHandpayDevice);
        }
    }
}