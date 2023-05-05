namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Newtonsoft.Json;
    using Protocol.v21;

    /// <summary>
    ///     The <i>cabinet</i> class includes commands and events related to the physical housing and security of the EGM as
    ///     well as the commands to enable, disable, and lock the EGM from play.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Typically disable means that something is wrong and that the device should not be used.This is different than a
    ///         lockout that freezes the machine. Typically an enable/disable feature should take the device out of service
    ///         and, as directed by the configuration  server, result in a cash-out. The lockout feature should freeze the game
    ///         after any play sequence currently in process completes.
    ///     </para>
    ///     <para>
    ///         The cabinet class also includes events related to components of the EGM that are not broken out into their own
    ///         device classes, such as service buttons, lamps, video displays, and so on.The cabinet class includes commands
    ///         and events related to the main processor board of the EGM and its ancillary components such as non-volatile
    ///         memory.It also includes the commands to set the current date/time.
    ///     </para>
    ///     <para>
    ///         The cabinet class is a single-device class. With the exception of class-level meters, the EGM MUST only expose
    ///         one active cabinet device within the cabinet, eventHandler, and meters classes.Other inactive devices within
    ///         the cabinet class may be reported through the commConfig class.
    ///     </para>
    /// </remarks>
    public class CabinetDevice : ClientDeviceBase<cabinet>, ICabinetDevice
    {
        private const int DefaultConfigDelayPeriod = 240000;

        private readonly List<Tuple<IDevice, EgmState, int>> _conditions = new List<Tuple<IDevice, EgmState, int>>();
        private readonly IEgmStateObserver _egmStateObserver;
        private readonly object _sync = new object();

        private IDevice _device;
        private int _faultId;
        private EgmState _state;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CabinetDevice" /> class.
        /// </summary>
        /// <param name="deviceObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="egmStateObserver">An <see cref="IEgmStateObserver" /> instance.</param>
        public CabinetDevice(IDeviceObserver deviceObserver, IEgmStateObserver egmStateObserver)
            : base(1, deviceObserver)
        {
            _egmStateObserver = egmStateObserver ?? throw new ArgumentNullException(nameof(egmStateObserver));

            SetDefaults();

            MoneyOutEnabled = true;
            Enabled = true;
        }

        /// <inheritdoc />
        public bool RestartStatus { get; private set; }

        /// <inheritdoc />
        public bool RestartStatusMode { get; private set; }

        /// <inheritdoc />
        [JsonIgnore]
        public bool GamePlayEnabled { get; set; }

        /// <inheritdoc />
        public int ConfigDelayPeriod { get; private set; }

        /// <inheritdoc />
        public bool MoneyOutEnabled { get; set; }

        /// <inheritdoc />
        public bool EnhancedConfigurationMode { get; private set; }

        /// <inheritdoc />
        public bool MasterResetAllowed { get; private set; }

        /// <inheritdoc />
        public bool ProcessorReset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public EgmState State
        {
            get
            {
                lock (_sync)
                {
                    return _state;
                }
            }

            private set => _state = value;
        }

        /// <inheritdoc />
        [JsonIgnore]
        public int FaultId
        {
            get
            {
                lock (_sync)
                {
                    return _faultId;
                }
            }

            private set => _faultId = value;
        }

        /// <inheritdoc />
        [JsonIgnore]
        public IDevice Device
        {
            get
            {
                lock (_sync)
                {
                    return _device;
                }
            }

            private set => _device = value;
        }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void Close()
        {
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            base.ApplyOptions(optionConfigValues);

            SetDeviceValue(
                G2SParametersNames.RestartStatusParameterName,
                optionConfigValues,
                parameterId => { RestartStatus = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.UseDefaultConfigParameterName,
                optionConfigValues,
                parameterId => { UseDefaultConfig = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.RequiredForPlayParameterName,
                optionConfigValues,
                parameterId => { RequiredForPlay = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.CabinetDevice.ConfigDelayPeriodParameterName,
                optionConfigValues,
                parameterId => { ConfigDelayPeriod = optionConfigValues.Int32Value(parameterId); });

            SetDeviceValue(
                G2SParametersNames.CabinetDevice.MasterResetAllowedParameterName,
                optionConfigValues,
                parameterId => { MasterResetAllowed = optionConfigValues.BooleanValue(parameterId); });
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE006); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE009);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE010);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE105); // Deprecated
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE106); // Deprecated

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE201);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE202);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE203);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE204);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE205);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE206);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE207);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE208);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE209);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE210);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE211);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE301);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE302);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE303);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE304);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE305);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE306);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE307);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE308);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE309);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE310);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE311); 
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE312); // Not used
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE313); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE314);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE315);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE316);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE317);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE318);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE319);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE320); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE321);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE322);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE323); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE324); //Deprecated but necessary for Vertex Validation
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE325);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE326);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE327);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE328);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE329);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE330); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE331); //// TODO

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE401);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE402);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE501); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE502); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CBE503); //// TODO

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE006);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE007);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE008);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE009);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE010);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE011);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.GTK_CBE012);
        }

        /// <inheritdoc />
        public void AddCondition(IDevice device, EgmState state)
        {
            AddCondition(device, state, -1);
        }

        /// <inheritdoc />
        public void AddCondition(IDevice device, EgmState state, int faultId)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (!Enum.IsDefined(typeof(EgmState), state))
            {
                throw new InvalidEnumArgumentException(nameof(state), (int)state, typeof(EgmState));
            }

            if (state == EgmState.Enabled)
            {
                throw new ArgumentException(@"The Enabled state cannot be specified.", nameof(state));
            }

            if (!device.RequiredForPlay && state != EgmState.HostLocked && state != EgmState.EgmLocked)
            {
                return;
            }

            lock (_sync)
            {
                var key = Tuple.Create(device, state, faultId);
                if (!_conditions.Contains(key))
                {
                    _conditions.Add(key);

                    _egmStateObserver.StateAdded(device, state, faultId);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveConditions(IDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            lock (_sync)
            {
                var conditions = _conditions.Where(c => c.Item1 == device).ToList();
                foreach (var condition in conditions)
                {
                    RemoveCondition(condition.Item1, condition.Item2, condition.Item3);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveCondition(IDevice device, EgmState state)
        {
            RemoveCondition(device, state, -1);
        }

        /// <inheritdoc />
        public void RemoveCondition(IDevice device, EgmState state, int faultId)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (!Enum.IsDefined(typeof(EgmState), state))
            {
                throw new InvalidEnumArgumentException(nameof(state), (int)state, typeof(EgmState));
            }

            if (state == EgmState.Enabled)
            {
                throw new ArgumentException(@"The Enabled state cannot be specified.", nameof(state));
            }

            lock (_sync)
            {
                var key = Tuple.Create(device, state, faultId);
                if (_conditions.Remove(key))
                {
                    _egmStateObserver.StateRemoved(device, state, faultId);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveAllConditions()
        {
            lock (_sync)
            {
                if (!_conditions.Any())
                {
                    Evaluate();
                }
            }
        }

        /// <inheritdoc />
        public bool Evaluate()
        {
            var updated = false;

            lock (_sync)
            {
                var key = Tuple.Create(Device, State, FaultId);
                var reset = !_conditions.Contains(key);

                var condition = _conditions.OrderBy(k => k.Item2).FirstOrDefault();

                if (condition == null && (Device != null || State != EgmState.Enabled))
                {
                    Device = null;
                    State = EgmState.Enabled;
                    FaultId = -1;
                    updated = true;
                }
                else if (condition != null && (reset || IsHigherPrecedence(State, condition.Item2)))
                {
                    Device = condition.Item1;
                    State = condition.Item2;
                    FaultId = condition.Item3;
                    updated = true;
                }

                var enabled = _conditions.All(c => c.Item2 != EgmState.EgmDisabled);
                if (enabled != Enabled)
                {
                    Enabled = enabled;

                    _egmStateObserver.NotifyEnabledChanged(this, Enabled);
                }
            }

            if (updated)
            {
                _egmStateObserver.NotifyStateChanged(Device, State, FaultId);
            }

            return updated;
        }

        /// <inheritdoc />
        public bool HasCondition(Func<IDevice, EgmState, int, bool> predicate)
        {
            lock (_sync)
            {
                return _conditions.Any(c => predicate.Invoke(c.Item1, c.Item2, c.Item3));
            }
        }

        /// <inheritdoc />
        public Session SendMasterResetStatus(object command)
        {
            var status = command as masterResetStatus;
            var request = InternalCreateClass();
            request.Item = status;
            return Queue.SendRequest(request);
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        private static bool IsHigherPrecedence(EgmState current, EgmState suggested)
        {
            return ToPrecedence(suggested) > ToPrecedence(current);
        }

        private static StatePrecedence ToPrecedence(EgmState state)
        {
            switch (state)
            {
                case EgmState.OperatorMode:
                case EgmState.AuditMode:
                    return StatePrecedence.OperatorControlled;
                case EgmState.OperatorDisabled:
                case EgmState.OperatorLocked:
                    return StatePrecedence.OperatorDisabled;
                case EgmState.TransportDisabled:
                case EgmState.HostDisabled:
                case EgmState.EgmDisabled:
                case EgmState.EgmLocked:
                case EgmState.HostLocked:
                    return StatePrecedence.EgmDisabled;
                case EgmState.DemoMode:
                    return StatePrecedence.DemoMode;
            }

            return StatePrecedence.Enabled;
        }

        private void SetDefaults()
        {
            RestartStatusMode = true;
            RestartStatus = true;
            RequiredForPlay = true;
            ConfigDelayPeriod = DefaultConfigDelayPeriod;
            EnhancedConfigurationMode = true;
            GamePlayEnabled = true;
            MasterResetAllowed = true;
        }

        private enum StatePrecedence
        {
            Enabled,
            DemoMode,
            EgmDisabled,
            OperatorDisabled,
            OperatorControlled
        }
    }
}
