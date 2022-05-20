namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     A device container
    /// </summary>
    internal class DeviceConnector : IDeviceConnector
    {
        private readonly ConcurrentDictionary<Tuple<string, int>, IDevice> _devices =
            new ConcurrentDictionary<Tuple<string, int>, IDevice>();

        /// <inheritdoc />
        public IEnumerable<IDevice> Devices => _devices.Values;

        /// <inheritdoc />
        public IDevice GetDevice(string deviceClass, int deviceId)
        {
            _devices.TryGetValue(Tuple.Create(deviceClass, deviceId), out var device);

            return device;
        }

        /// <inheritdoc />
        public TDevice GetDevice<TDevice>()
            where TDevice : IDevice
        {
            return (TDevice)_devices.Values.SingleOrDefault(device => device is TDevice);
        }

        /// <inheritdoc />
        public IEnumerable<TDevice> GetDevices<TDevice>()
            where TDevice : IDevice
        {
            return _devices.Values.Where(device => device is TDevice).Cast<TDevice>();
        }

        /// <inheritdoc />
        public TDevice GetDevice<TDevice>(int deviceId)
            where TDevice : IDevice
        {
            return (TDevice)_devices.Values.FirstOrDefault(device => device.Id == deviceId && device is TDevice);
        }

        /// <inheritdoc />
        public IDevice AddDevice(IDevice device)
        {
            if (device.IsSingle() && Devices.Any(d => d.DeviceClass == device.DeviceClass))
            {
                throw new ArgumentException(
                    $@"Only one instance of a {device.DeviceClass} device can be registered.",
                    nameof(device));
            }

            var key = Tuple.Create(device.DeviceClass, device.Id);

            var added = _devices.AddOrUpdate(key, device, (id, d) => device);

            added?.RegisterEvents();

            return added;
        }

        /// <inheritdoc />
        public IDevice RemoveDevice(IDevice device)
        {
            var key = Tuple.Create(device.DeviceClass, device.Id);

            _devices.TryRemove(key, out var deletedDevice);

            deletedDevice?.UnregisterEvents();

            return deletedDevice;
        }
    }
}
