namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.SharedDevice;
    using Kernel;

    /// <summary>
    ///     An implementation of <see cref="IDeviceRegistryService" />
    /// </summary>
    public class DeviceRegistryService : IDeviceRegistryService
    {
        private readonly IServiceManager _services;

        public DeviceRegistryService()
            : this(ServiceManager.GetInstance())
        {
        }

        public DeviceRegistryService(IServiceManager services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IDeviceRegistryService) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public T GetDevice<T>()
        {
            return _services.TryGetService<T>();
        }

        /// <inheritdoc />
        public void AddDevice<T>(T device) where T : IDeviceService
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (_services.IsServiceAvailable<T>())
            {
                return;
            }

            _services.AddService(device);
        }

        /// <inheritdoc />
        public void RemoveDevice<T>() where T : IDeviceService
        {
            var device = GetDevice<T>();
            if (device == null)
            {
                return;
            }

            RemoveDevice(device);
        }

        /// <inheritdoc />
        public void RemoveDevice<T>(T device) where T : IDeviceService
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            _services.RemoveService(device);
        }
    }
}