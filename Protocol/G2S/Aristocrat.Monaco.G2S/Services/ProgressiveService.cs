namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Timers;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Handlers;
    using Handlers.Progressive;
    using Kernel;
    using Localization.Properties;
    using log4net;

    public class ProgressiveService : IService, IDisposable, IProtocolProgressiveEventHandler
    {
        private const int DefaultNoProgInfo = 30000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ICommandBuilder<IProgressiveDevice, progressiveStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IEventLift _eventLift;
        private readonly IGameProvider _gameProvider;

        private bool _commsDisable = true;
        private bool _disposed;
        private bool _levelMismatch;
        private bool _progressiveStateDisable = true;
        private bool _progressiveValue = true;
        private Timer _progressiveValueUpdateTimer;
        private readonly IProtocolProgressiveEventsRegistry _protocolProgressiveEventsRegistry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveService" /> class.
        /// </summary>
        public ProgressiveService(
            IG2SEgm egm,
            IEventLift eventLift,
            IEventBus eventBus,
            ICommandBuilder<IProgressiveDevice, progressiveStatus> statusCommandBuilder,
            IGameProvider gameProvider,
            IProtocolProgressiveEventsRegistry protocolProgressiveEventSubscriber)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _commandBuilder = statusCommandBuilder ?? throw new ArgumentNullException(nameof(statusCommandBuilder));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolProgressiveEventsRegistry = protocolProgressiveEventSubscriber ??
                                                  throw new ArgumentNullException(
                                                      nameof(protocolProgressiveEventSubscriber));
            SubscribeEvents();
            _progressiveValueUpdateTimer = new Timer(DefaultNoProgInfo);
            _progressiveValueUpdateTimer.Elapsed += EventUpdateTimerElapsed;
            LastProgressiveUpdateTime = DateTime.UtcNow;
            _progressiveValueUpdateTimer.Start();
        }

        /// <summary>
        ///     Last update SetProgressive Value Received Time
        /// </summary>
        public DateTime LastProgressiveUpdateTime { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ProgressiveService) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Initializing the G2S VoucherDataService.");
        }

        /// <summary>
        ///     Level [Matched/Mismatched] lockup.
        /// </summary>
        public void LevelMismatchLockup(bool deviceEnabled)
        {
            var device = _egm.GetDevice<IProgressiveDevice>();
            if (!device.HostEnabled && !device.Enabled)
            {
                return;
            }

            if (deviceEnabled)
            {
                EnableProgressiveDevice(DeviceDisableReason.LevelMismatch);
            }
            else
            {
                DisableProgressiveDevice(DeviceDisableReason.LevelMismatch);
            }
        }

        /// <summary>
        ///     ProgressiveValue Timeout lockup.
        /// </summary>
        public void ProgressiveValueTimeoutLockup(bool deviceEnabled)
        {
            var device = _egm.GetDevice<IProgressiveDevice>();
            if (!device.HostEnabled && !device.Enabled)
            {
                return;
            }

            if (deviceEnabled)
            {
                EnableProgressiveDevice(DeviceDisableReason.ProgressiveValue);
            }
            else
            {
                DisableProgressiveDevice(DeviceDisableReason.ProgressiveValue);
            }
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
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
                _protocolProgressiveEventsRegistry.UnSubscribeProgressiveEvent<ProgressiveCommitEvent>(
                    ProtocolNames.G2S, this);
                _protocolProgressiveEventsRegistry.UnSubscribeProgressiveEvent<ProgressiveCommitAckEvent>(
                    ProtocolNames.G2S, this);
                _protocolProgressiveEventsRegistry.UnSubscribeProgressiveEvent<ProgressiveHitEvent>(
                    ProtocolNames.G2S, this);

                _progressiveValueUpdateTimer.Stop();
                _progressiveValueUpdateTimer.Dispose();
            }

            _progressiveValueUpdateTimer = null;

            _disposed = true;
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<HostUnreachableEvent>(this, CommunicationsStateChanged);
            _eventBus.Subscribe<TransportDownEvent>(this, CommunicationsStateChanged);
            _eventBus.Subscribe<TransportUpEvent>(this, CommunicationsStateChanged);
            _protocolProgressiveEventsRegistry.SubscribeProgressiveEvent<ProgressiveCommitEvent>(
                ProtocolNames.G2S, this);
            _protocolProgressiveEventsRegistry.SubscribeProgressiveEvent<ProgressiveCommitAckEvent>(
                ProtocolNames.G2S, this);
            _protocolProgressiveEventsRegistry.SubscribeProgressiveEvent<ProgressiveHitEvent>(
                ProtocolNames.G2S, this);
        }

        private void EventUpdateTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _progressiveValueUpdateTimer?.Stop();

            var device = _egm.GetDevice<IProgressiveDevice>();

            var noProgressiveInfo = device.NoProgressiveInfo;

            if (noProgressiveInfo <= 0)
            {
                noProgressiveInfo = DefaultNoProgInfo;
            }

            var timeout = TimeSpan.FromMilliseconds(noProgressiveInfo);

            if (DateTime.UtcNow - LastProgressiveUpdateTime > timeout)
            {
                ProgressiveValueTimeoutLockup(false);
            }

            if (_progressiveValueUpdateTimer != null && timeout.TotalMilliseconds > 0)
            {
                _progressiveValueUpdateTimer.Interval = timeout.TotalMilliseconds;
            }

            _progressiveValueUpdateTimer?.Start();
        }

        private void OnTransportUp()
        {
            var progressiveHostInfo = GetProgressiveHostInfo();

            if (progressiveHostInfo == null)
            {
                Logger.Info("ProgressiveHostInfo is not found");
                return;
            }

            var progressiveDevices = _egm.GetDevices<IProgressiveDevice>();

            foreach (var progressiveDevice in progressiveDevices)
            {
                var matchedProgressive = progressiveHostInfo.progressiveLevel.Where(
                    progressiveLevel => progressiveLevel.progId.Equals(progressiveDevice.Id)).ToList();

                if (!matchedProgressive.Any())
                {
                    progressiveDevice.Enabled = false;
                    DisableProgressiveDevice(DeviceDisableReason.CommsOnline);
                    return;
                }

                //var progressive = _progressiveProvider.GetProgressive(progressiveDevice.Id);

                foreach (var level in matchedProgressive)
                {
                    IViewableProgressiveLevel matchedLevel = null;

                    if (matchedLevel == null)
                    {
                        LevelMismatchLockup(false);
                        return;
                    }
                }

                EnableProgressiveDevice(DeviceDisableReason.CommsOnline);
                LevelMismatchLockup(true);
            }
        }

        private progressiveHostInfo GetProgressiveHostInfo()
        {
            var timeout = TimeSpan.MaxValue;
            var currentUtcNow = DateTime.UtcNow;
            var progressiveDevice = _egm.GetDevice<IProgressiveDevice>();
            var command = new getProgressiveHostInfo();

            var progressiveHostInfo = progressiveDevice.GetProgressiveHostInfo(command, timeout);
            if (progressiveHostInfo == null)
            {
                if (DateTime.UtcNow - currentUtcNow > timeout)
                {
                    Logger.Info($"Command was unsuccessful, posting {EventCode.G2S_PGE106} event");
                }

                return null;
            }

            if (!progressiveHostInfo.IsValid())
            {
                Logger.Info("Received ProgressiveHostInfo  is invalid");

                return null;
            }

            return progressiveHostInfo;
        }

        private void SetAllGamePlayDeviceState(bool state)
        {
            var gamePlayDevices = _egm.GetDevices<IGamePlayDevice>();

            foreach (var gamePlayDevice in gamePlayDevices)
            {
                gamePlayDevice.Enabled = state;
            }
        }

        private void DisableProgressiveDevice(DeviceDisableReason reason)
        {
            switch (reason)
            {
                case DeviceDisableReason.LevelMismatch:
                    _levelMismatch = true;
                    break;
                case DeviceDisableReason.CommsOnline:
                    _commsDisable = true;
                    break;
                case DeviceDisableReason.ProgressiveState:
                    _progressiveStateDisable = true;
                    break;
                case DeviceDisableReason.ProgressiveValue:
                    _progressiveValue = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }

            var device = _egm.GetDevice<IProgressiveDevice>();

            if (device.Enabled)
            {
                SetAllGamePlayDeviceState(false);

                if (_commsDisable)
                {
                    device.DisableText = Resources.ProgressiveDisconnectText;
                }
                else if (_levelMismatch)
                {
                    device.DisableText = Resources.ProgressiveLevelMismatchText;
                }

                Logger.Debug($"Progressive service is disabling the Progressive device: {reason}");

                device.Enabled = false;

                var status = new progressiveStatus();
                _commandBuilder.Build(device, status);

                _eventLift.Report(device, EventCode.G2S_PGE001, device.DeviceList(status));
            }
        }

        /// <summary>
        ///     To enable device behalf on reason
        /// </summary>
        /// <param name="reason">device enable reason</param>
        private void EnableProgressiveDevice(DeviceDisableReason reason)
        {
            switch (reason)
            {
                case DeviceDisableReason.LevelMismatch:
                    _levelMismatch = false;
                    break;
                case DeviceDisableReason.CommsOnline:
                    _commsDisable = false;
                    break;
                case DeviceDisableReason.ProgressiveState:
                    _progressiveStateDisable = false;
                    break;
                case DeviceDisableReason.ProgressiveValue:
                    _progressiveValue = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }

            var device = _egm.GetDevice<IProgressiveDevice>();

            if (!device.Enabled)
            {
                Logger.Debug($"Progressive service is enabling the Progressive device: {reason}");

                if (!_commsDisable && !_progressiveStateDisable && !_levelMismatch && !_progressiveValue)
                {
                    SetAllGamePlayDeviceState(true);
                    device.Enabled = true;

                    var status = new progressiveStatus();
                    _commandBuilder.Build(device, status);

                    _eventLift.Report(device, EventCode.G2S_PGE002, device.DeviceList(status));
                }
            }
        }

        private void CommunicationsStateChanged(IEvent theEvent)
        {
            if (theEvent.GetType() == typeof(TransportUpEvent))
            {
                EnableProgressiveDevice(DeviceDisableReason.CommsOnline);
                // TO DO - getProgressiveHostInfo is getting called here ,once TransportUpEvent received assuming Host is connected and up.
                Task.Run(() => OnTransportUp()); // call getProgressiveHostInfo
            }
            else
            {
                DisableProgressiveDevice(DeviceDisableReason.CommsOnline);
            }
        }

        private void HandleEvent(ProgressiveCommitEvent evt)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(evt.Jackpot.DeviceId);

            if (evt.Jackpot.State != ProgressiveState.Committed)
            {
                return;
            }

            EventReport(device, evt.Jackpot, EventCode.G2S_PGE104);
        }

        private void HandleEvent(ProgressiveCommitAckEvent evt)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(evt.Jackpot.DeviceId);

            if (evt.Jackpot.State != ProgressiveState.Acknowledged)
            {
                return;
            }

            EventReport(device, evt.Jackpot, EventCode.G2S_PGE105);
        }

        private void HandleEvent(ProgressiveHitEvent evt)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(evt.Jackpot.DeviceId);

            if (evt.Jackpot.State != ProgressiveState.Hit)
            {
                return;
            }

            EventReport(device, evt.Jackpot, EventCode.G2S_PGE102);
        }

        /*
        private void HandleEvent(ProgressiveAddedEvent @event)
        {
            _deviceFactory.Create(
                _egm.GetHostById(Constants.EgmHostId),
                () => new ProgressiveDevice(@event.DeviceId, _deviceObserver));
        }

        private void HandleEvent(ProgressiveRemovedEvent @event)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(@event.DeviceId);
            if (device == null)
            {
                return;
            }

            _egm.RemoveDevice(device);
        }
        */

        private void EventReport(IProgressiveDevice device, JackpotTransaction log, string eventCode)
        {
            _eventLift.Report(
                device,
                eventCode,
                log.TransactionId,
                new transactionList
                {
                    transactionInfo = new[]
                    {
                        new transactionInfo
                        {
                            deviceId = device.Id,
                            deviceClass = device.PrefixedDeviceClass(),
                            Item = log.ToProgressiveLog(_gameProvider)
                        }
                    }
                });
        }

        private enum DeviceDisableReason
        {
            CommsOnline,
            ProgressiveState,
            LevelMismatch,
            ProgressiveValue
        }

        public void HandleProgressiveEvent<T>(T data)
        {
            switch (data)
            {
                case ProgressiveCommitEvent commitEvent:
                    HandleEvent(commitEvent);
                    break;
                case ProgressiveCommitAckEvent commitAckEvent:
                    HandleEvent(commitAckEvent);
                    break;
                case ProgressiveHitEvent hitEvent:
                    HandleEvent(hitEvent);
                    break;
            }
        }
    }
}