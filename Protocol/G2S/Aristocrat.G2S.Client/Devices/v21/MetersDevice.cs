namespace Aristocrat.G2S.Client.Devices.v21
{
    using Protocol.v21;

    /// <summary>
    ///     G2S Meters device
    /// </summary>
    public class MetersDevice : HostOrientedDevice<meters>, IMetersDevice
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MetersDevice" /> class.
        /// </summary>
        /// <param name="deviceId">The device identifier, which for this class should be the host identifier.</param>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        public MetersDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : base(deviceId, deviceStateObserver)
        {
            ConfigComplete = true;
        }

        /// <inheritdoc />
        public override void Close()
        {
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
        }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public (bool, int) SendMeterInfo(meterInfo command, bool waitForAck)
        {
            if (!Queue.CanSend)
            {
                return (false, (int)Queue.SessionTimeout.TotalMilliseconds);
            }

            var request = InternalCreateClass();
            request.Item = command;

            if (!waitForAck)
            {
                SendNotification(request);
                return (true, 0);
            }

            var session = SendRequest(request, null, 0);
            session.WaitForCompletion();
            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0 &&
                session.Responses[0].IClass.Item is meterInfoAck)
            {
                return (true, 0);
            }

            if (session.SessionState == SessionStatus.TimedOut)
            {
                return (false, 0);
            }

            return (false, request.timeToLive);
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_MTE100);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_MTE101);
        }
    }
}