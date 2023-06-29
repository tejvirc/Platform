namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using Contracts.IO;
    using Contracts.KeySwitch;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using DisabledEvent = Contracts.KeySwitch.DisabledEvent;
    using EnabledEvent = Contracts.KeySwitch.EnabledEvent;

    /// <summary>
    ///     Provides implementation of key switch services. This component handles mapping and handling of physical input
    ///     events
    ///     from one or more IO services and posting the associated logical events to the system. Also provide an interface for
    ///     access
    ///     from operator menu.
    /// </summary>
    public class KeySwitchService : IDeviceService, IKeySwitch, IDisposable
    {
        // The minimum amount of time a key must be held ON to consider it a 'held' key
        private const double MinimumHeldKeyTime = 5000.0;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _bus;
        private readonly IIO _io;

        private readonly Dictionary<int, LogicalKeySwitchData> _logicalKeySwitchesById = new();

        private readonly Dictionary<Timer, LogicalKeySwitchData> _logicalKeySwitchesByTimer = new();

        private bool _disposed;

        public KeySwitchService(
            IEventBus bus,
            IIO io)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _io = io ?? throw new ArgumentNullException(nameof(io));

            Enabled = false;
            Initialized = false;
        }

        private KeySwitchLogicalState LastEnabledLogicalState { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; private set; }

        /// <inheritdoc />
        public bool Initialized { get; private set; }

        /// <inheritdoc />
        public string LastError { get; private set; }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public DisabledReasons ReasonDisabled { get; private set; }

        /// <inheritdoc />
        public string ServiceProtocol
        {
            get => string.Empty;
            set { }
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public void Disable(DisabledReasons reason)
        {
            Logger.Info(Name + " disabled by " + reason);
            ReasonDisabled |= reason;
            Enabled = false;
            if (LogicalState != KeySwitchLogicalState.Disabled)
            {
                Logger.Debug("Last enabled logical state " + LastEnabledLogicalState + " set to " + LogicalState);
                LastEnabledLogicalState = LogicalState;

                foreach (var data in _logicalKeySwitchesById.Values)
                {
                    data.Timer.Enabled = false;
                    data.KeySwitch.State = KeySwitchState.Disabled;
                }
            }

            LogicalState = KeySwitchLogicalState.Disabled;
            _bus.Publish(new DisabledEvent(ReasonDisabled));
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public bool Enable(EnabledReasons reason)
        {
            if (Enabled)
            {
                Logger.Debug(Name + " enabled by " + reason + " logical state " + LogicalState);
                _bus.Publish(new EnabledEvent(reason));
                return true;
            }

            if (!Initialized)
            {
                Logger.Debug(Name + " can not be enabled by " + reason + " because service is not initialized");
                _bus.Publish(new DisabledEvent(ReasonDisabled));
                return false;
            }

            if (((ReasonDisabled & DisabledReasons.Error) > 0 ||
                 (ReasonDisabled & DisabledReasons.FirmwareUpdate) > 0) &&
                (reason == EnabledReasons.Reset || reason == EnabledReasons.Operator))
            {
                if ((ReasonDisabled & DisabledReasons.Error) > 0)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.Error);
                    ReasonDisabled &= ~DisabledReasons.Error;
                }

                if ((ReasonDisabled & DisabledReasons.FirmwareUpdate) > 0)
                {
                    Logger.Debug("Removed disabled reason " + DisabledReasons.FirmwareUpdate);
                    ReasonDisabled &= ~DisabledReasons.FirmwareUpdate;
                }
            }
            else if ((ReasonDisabled & DisabledReasons.Operator) > 0 && reason == EnabledReasons.Operator)
            {
                Logger.Debug("Removed disabled reason " + DisabledReasons.Operator);
                ReasonDisabled &= ~DisabledReasons.Operator;
            }
            else if ((ReasonDisabled & DisabledReasons.Service) > 0 && reason == EnabledReasons.Service)
            {
                Logger.Debug("Removed disabled reason " + DisabledReasons.Service);
                ReasonDisabled &= ~DisabledReasons.Service;
            }
            else if ((ReasonDisabled & DisabledReasons.System) > 0 && reason == EnabledReasons.System)
            {
                Logger.Debug("Removed disabled reason " + DisabledReasons.System);
                ReasonDisabled &= ~DisabledReasons.System;
            }
            else if ((ReasonDisabled & DisabledReasons.Configuration) > 0 && reason == EnabledReasons.Configuration)
            {
                Logger.Debug("Removed disabled reason " + DisabledReasons.Configuration);
                ReasonDisabled &= ~DisabledReasons.Configuration;
            }

            // Set enabled if we no longer have any disabled reasons.
            Enabled = ReasonDisabled == 0;
            if (Enabled)
            {
                Logger.Debug(Name + " enabled by " + reason + " logical state " + LogicalState);
                if (LastEnabledLogicalState != KeySwitchLogicalState.Uninitialized &&
                    LastEnabledLogicalState != KeySwitchLogicalState.Idle)
                {
                    Logger.Debug("Logical state " + LogicalState + " reset to " + KeySwitchLogicalState.Idle);
                    LogicalState = KeySwitchLogicalState.Idle;
                }
                else
                {
                    Logger.Debug("Logical state " + LogicalState + " reset to " + LastEnabledLogicalState);
                    LogicalState = LastEnabledLogicalState;
                }

                foreach (var data in _logicalKeySwitchesById.Values)
                {
                    data.KeySwitch.State = KeySwitchState.Enabled;
                    data.KeyEvaluated = true;
                }

                _bus.Publish(new EnabledEvent(reason));
            }
            else
            {
                Logger.Debug(Name + " can not be enabled by " + reason + " because disabled by " + ReasonDisabled);
                _bus.Publish(new DisabledEvent(ReasonDisabled));
            }

            return Enabled;
        }

        /// <inheritdoc />
        public string Name => "KeySwitch Service";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IKeySwitch) };

        /// <inheritdoc />
        public void Initialize()
        {
            if (Initialized)
            {
                return;
            }

            Logger.Info("Initializing...");

            LastError = string.Empty;

            foreach (var key in _io.GetConfiguration().KeySwitches)
            {
                var timer = new Timer(MinimumHeldKeyTime) { AutoReset = false };
                timer.Elapsed += HandleKeySwitchTimerElapsed;

                var keySwitch =
                    new LogicalKeySwitch(key.PhysicalId, key.Name, key.Name) { State = KeySwitchState.Enabled };

                var data = new LogicalKeySwitchData(key.LogicalId, keySwitch, timer);

                _logicalKeySwitchesById.Add(key.LogicalId, data);
                _logicalKeySwitchesByTimer.Add(timer, data);

                Logger.Debug($"Added KeySwitch physicalID {key.PhysicalId}, logicalID {key.LogicalId} - {key.Name}");

                if (LogicalState == KeySwitchLogicalState.Uninitialized)
                {
                    LogicalState = KeySwitchLogicalState.Idle;
                }
            }

            _bus.Subscribe<InputEvent>(this, ReceiveEvent);

            Initialized = true;

            Enable(EnabledReasons.Service);

            _bus.Publish(new KeySwitchServiceInitializedEvent());

            Logger.Info("Initialized");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public Dictionary<int, LogicalKeySwitch> LogicalKeySwitches
        {
            get { return _logicalKeySwitchesById.ToDictionary(pair => pair.Key, pair => pair.Value.KeySwitch); }
        }

        /// <inheritdoc />
        public KeySwitchLogicalState LogicalState { get; private set; }

        /// <inheritdoc />
        public void Disable(Collection<int> keySwitchIdList)
        {
            foreach (var keySwitchId in keySwitchIdList)
            {
                if (!_logicalKeySwitchesById.TryGetValue(keySwitchId, out var keySwitchData))
                {
                    Logger.WarnFormat("Cannot disable logical KeySwitch {0}, need to add first", keySwitchId);
                    return;
                }

                keySwitchData.Timer.Enabled = false;

                var keySwitch = keySwitchData.KeySwitch;
                if (keySwitch.State == KeySwitchState.Enabled)
                {
                    keySwitch.State = KeySwitchState.Disabled;
                    Logger.DebugFormat(
                        CultureInfo.CurrentCulture,
                        "Logical KeySwitch {0} {1} {2}",
                        keySwitchId,
                        keySwitch.Name,
                        keySwitch.State);
                }
            }
        }

        public void Enable(Collection<int> keySwitchIdList)
        {
            foreach (var keySwitchId in keySwitchIdList)
            {
                if (!_logicalKeySwitchesById.TryGetValue(keySwitchId, out var keySwitchData))
                {
                    Logger.ErrorFormat("Cannot enable logical KeySwitch {0}, need to add first", keySwitchId);
                    return;
                }

                keySwitchData.KeyEvaluated = true;

                var keySwitch = keySwitchData.KeySwitch;
                if (keySwitch.State == KeySwitchState.Disabled)
                {
                    keySwitch.State = KeySwitchState.Enabled;
                    Logger.DebugFormat(
                        CultureInfo.CurrentCulture,
                        "Logical KeySwitch {0} {1} {2}",
                        keySwitchId,
                        keySwitch.Name,
                        keySwitch.State);
                }
            }
        }

        /// <inheritdoc />
        public KeySwitchAction GetKeySwitchAction(int keySwitchId)
        {
            return _logicalKeySwitchesById.TryGetValue(keySwitchId, out var keySwitchData)
                ? keySwitchData.KeySwitch.Action
                : KeySwitchAction.Off;
        }

        /// <inheritdoc />
        public int GetKeySwitchId(string keySwitchName)
        {
            var key = _logicalKeySwitchesById.FirstOrDefault(k => k.Value.KeySwitch.Name == keySwitchName);

            return key.Key;
        }

        /// <inheritdoc />
        public string GetKeySwitchName(int keySwitchId)
        {
            var name = string.Empty;

            return _logicalKeySwitchesById.TryGetValue(keySwitchId, out var keySwitchData)
                ? keySwitchData.KeySwitch.Name
                : name;
        }

        /// <inheritdoc />
        public int GetKeySwitchPhysicalId(int keySwitchId)
        {
            return _logicalKeySwitchesById.TryGetValue(keySwitchId, out var keySwitchData)
                ? keySwitchData.KeySwitch.PhysicalId
                : -1;
        }

        /// <inheritdoc />
        public KeySwitchState GetKeySwitchState(int keySwitchId)
        {
            return _logicalKeySwitchesById.TryGetValue(keySwitchId, out var keySwitchData)
                ? keySwitchData.KeySwitch.State
                : KeySwitchState.Uninitialized;
        }

        /// <inheritdoc />
        public string GetLocalizedKeySwitchName(int keySwitchId)
        {
            return _logicalKeySwitchesById.TryGetValue(keySwitchId, out var keySwitchData)
                ? keySwitchData.KeySwitch.LocalizedName
                : string.Empty;
        }

        /// <summary>Disposes the service.</summary>
        /// <param name="disposing">Whether or not managed resources should be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus?.UnsubscribeAll(this);

                Disable(DisabledReasons.Service);

                foreach (var data in _logicalKeySwitchesById.Values)
                {
                    data.Timer.Enabled = false;
                    data.Timer.Elapsed -= HandleKeySwitchTimerElapsed;
                    data.Timer.Dispose();
                    data.KeyEvaluated = true;
                }

                _logicalKeySwitchesById.Clear();
                _logicalKeySwitchesByTimer.Clear();
            }

            _disposed = true;
        }

        private void HandleKeySwitchOn(LogicalKeySwitchData logicalKeySwitchData)
        {
            _bus.Publish(
                new OnEvent(
                    logicalKeySwitchData.LogicalId,
                    LogicalKeySwitches[logicalKeySwitchData.LogicalId].LocalizedName));

            if (logicalKeySwitchData.KeyEvaluated)
            {
                Logger.DebugFormat(
                    CultureInfo.InvariantCulture,
                    "Logical Key {0} ON event ignored - already evaluated",
                    logicalKeySwitchData.LogicalId);
            }
            else
            {
                logicalKeySwitchData.KeyEvaluated = true;
            }
        }

        private void HandleKeySwitchOff(LogicalKeySwitchData logicalKeySwitchData)
        {
            _bus.Publish(
                new OffEvent(
                    logicalKeySwitchData.LogicalId,
                    LogicalKeySwitches[logicalKeySwitchData.LogicalId].LocalizedName));

            if (logicalKeySwitchData.KeyEvaluated)
            {
                logicalKeySwitchData.KeyEvaluated = false;

                Logger.DebugFormat("Logical Key {0} turned", logicalKeySwitchData.LogicalId);
            }
            else
            {
                Logger.DebugFormat(
                    "Logical Key {0} OFF event ignored - key still being evaluated",
                    logicalKeySwitchData.LogicalId);
            }
        }

        private void HandleKeySwitchTimerElapsed(object sender, ElapsedEventArgs args)
        {
            // Could be possible that this object was disposed after this call was scheduled
            if (_disposed)
            {
                Logger.Info("HandleKeySwitchTimerElapsed() called after being disposed");
                return;
            }

            if (!_logicalKeySwitchesByTimer.TryGetValue((Timer)sender, out var logicalKeySwitchData))
            {
                Logger.Warn("HandleKeySwitchTimerElapsed() for timer not in dictionary");
                return;
            }

            // Ignore if switch is disabled
            if (logicalKeySwitchData.KeySwitch.State != KeySwitchState.Enabled)
            {
                Logger.DebugFormat(
                    "HandleKeySwitchTimerElapsed() ignored for disabled logical key switch {0}",
                    logicalKeySwitchData.LogicalId);
                return;
            }

            if (logicalKeySwitchData.KeyEvaluated)
            {
                Logger.DebugFormat(
                    CultureInfo.InvariantCulture,
                    "Key {0} timer expiration ignored - already evaluated",
                    logicalKeySwitchData.LogicalId);
            }
            else
            {
                logicalKeySwitchData.KeyEvaluated = true;
                _bus.Publish(
                    new KeyHeldEvent(
                        logicalKeySwitchData.LogicalId,
                        LogicalKeySwitches[logicalKeySwitchData.LogicalId].LocalizedName));
                Logger.DebugFormat("Logical Key {0} held", logicalKeySwitchData.LogicalId);
            }
        }

        private void ReceiveEvent(InputEvent evt)
        {
            var key = _logicalKeySwitchesById.FirstOrDefault(k => k.Value.KeySwitch.PhysicalId == evt.Id);

            var keySwitchData = key.Value;
            if (keySwitchData == null)
            {
                return;
            }

            var keySwitch = keySwitchData.KeySwitch;

            keySwitch.Action = evt.Action ? KeySwitchAction.On : KeySwitchAction.Off;

            // If disabled, do not send any logical events
            if (keySwitch.State != KeySwitchState.Enabled)
            {
                Logger.DebugFormat(
                    "Physical KeySwitch {0} {1}, logical KeySwitch {2} {3} disabled",
                    evt.Id,
                    keySwitch.Action,
                    key,
                    keySwitch.Name);
                return;
            }

            if (keySwitch.Action == KeySwitchAction.Off)
            {
                HandleKeySwitchOff(keySwitchData);
            }
            else
            {
                HandleKeySwitchOn(keySwitchData);
            }

            Logger.DebugFormat(
                "Physical KeySwitch {0} {1}, logical KeySwitch {2} {3} {4} event posted",
                evt.Id,
                keySwitch.Action,
                key,
                keySwitch.Name,
                keySwitch.Action);
        }

        private class LogicalKeySwitchData
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="LogicalKeySwitchData" /> class.
            /// </summary>
            /// <param name="id">The switch's logical ID</param>
            /// <param name="keySwitch">The key switch object</param>
            /// <param name="timer">The timer for determining turned/held behavior.</param>
            public LogicalKeySwitchData(int id, LogicalKeySwitch keySwitch, Timer timer)
            {
                LogicalId = id;
                KeySwitch = keySwitch;
                Timer = timer;
                KeyEvaluated = false;
            }

            /// <summary>Gets the logical ID</summary>
            public int LogicalId { get; }

            /// <summary>Gets the key switch object</summary>
            public LogicalKeySwitch KeySwitch { get; }

            /// <summary>Gets the Timer used for determining turned/held behavior</summary>
            public Timer Timer { get; }

            /// <summary>Gets or sets a value indicating whether or not the turned/held evaluation has happened.</summary>
            public bool KeyEvaluated { get; set; }
        }
    }
}