namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using Protocol.v21;

    /// <summary>
    ///     The commConfig class is used to remotely configure the G2S communications setup of an EGM. The class
    ///     includes commands for setting the list of registered hosts and assigning owners to devices.There is no
    ///     requirement that an EGM support the commConfig class. Alternatively, options covered by the commConfig
    ///     class can be set manually through the operator mode of the EGM; however, the commConfig class provides a
    ///     convenient and efficient mechanism to perform these functions remotely.
    ///     The commConfig class is a single-device class. The EGM MUST expose only one active commConfig device.
    ///     The device identifiers for devices within the communications, eventHandler, meters, optionConfig, and gat
    ///     classes MUST be equal to the host identifier of the host that owns the device.These classes are referred to as
    ///     host-oriented classes; devices within those classes are referred to as host-oriented devices.When a host is
    ///     registered, the EGM MUST create and remove host-oriented devices, as required.When a host is unregistered,
    ///     the EGM MUST remove all host-oriented devices owned by the host. The EGM never owns host-oriented
    ///     devices
    /// </summary>
    public class CommConfigDevice : ClientDeviceBase<commConfig>, ICommConfigDevice
    {
        /// <summary>
        ///     Default no response timer
        /// </summary>
        public const int DefaultNoResponseTimer = 300000;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommConfigDevice" /> class.
        /// </summary>
        /// <param name="deviceObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public CommConfigDevice(IDeviceObserver deviceObserver)
            : base(1, deviceObserver)
        {
            SetDefaults();
        }

        /// <inheritdoc />
        public int MinLogEntries { get; protected set; }

        /// <inheritdoc />
        public TimeSpan NoResponseTimer { get; protected set; }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void Close()
        {
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE007);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE008);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE105);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE106);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE107);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE108);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE109);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CCE110);
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            base.ApplyOptions(optionConfigValues);

            SetDeviceValue(
                G2SParametersNames.NoResponseTimerParameterName,
                optionConfigValues,
                parameterId => { NoResponseTimer = optionConfigValues.TimeSpanValue(parameterId); });
        }

        /// <inheritdoc />
        public void UpdateHostList(commHostList hostList)
        {
            var request = InternalCreateClass();
            request.Item = hostList;

            var session = SendRequest(request);
            session.WaitForCompletion();
        }

        /// <inheritdoc />
        public void CommChangeStatus(commChangeStatus changeStatus)
        {
            var request = InternalCreateClass();
            request.Item = changeStatus;

            var session = SendRequest(request);
            session.WaitForCompletion();
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        private void SetDefaults()
        {
            MinLogEntries = Constants.DefaultMinLogEntries;
            NoResponseTimer = TimeSpan.FromMilliseconds(DefaultNoResponseTimer);
        }
    }
}