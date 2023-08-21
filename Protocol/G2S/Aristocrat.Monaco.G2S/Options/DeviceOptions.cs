namespace Aristocrat.Monaco.G2S.Options
{
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;

    public abstract class DeviceOptions<TDevice> : BaseDeviceOptions
        where TDevice : IDevice
    {
        private readonly DeviceClass _deviceClass;

        protected DeviceOptions(DeviceClass deviceClass)
        {
            _deviceClass = deviceClass;
        }

        /// <inheritdoc />
        public override bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == _deviceClass;
        }
    }
}
