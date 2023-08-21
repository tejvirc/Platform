namespace Aristocrat.G2S.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Devices;

    /// <summary>
    ///     A set of host extension methods
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        ///     Determines if this host is the EGM.
        /// </summary>
        /// <param name="this">The host.</param>
        /// <returns>true if this host is the EGM.</returns>
        public static bool IsEgm(this IHost @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.Index == Constants.EgmHostIndex && @this.Id == Constants.EgmHostId;
        }

        /// <summary>
        ///     Assigns or removes this host as the owner.
        /// </summary>
        /// <param name="this">The host.</param>
        /// <param name="devices">The list of available devices.</param>
        /// <param name="owned">The list of devices for which this host is the owner.</param>
        /// <returns>a list of affected devices.</returns>
        internal static IEnumerable<IDevice> Owns(
            this IHost @this,
            IEnumerable<IDevice> devices,
            IEnumerable<OwnedDevice> owned)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (devices == null)
            {
                throw new ArgumentNullException(nameof(devices));
            }

            var affectedDevices = new List<IDevice>();

            var deviceList = devices.ToList();
            var ownedDevices = owned?.ToList() ?? Enumerable.Empty<OwnedDevice>().ToList();

            foreach (var device in deviceList
                .Where(d => !d.IsOwner(@this.Id) && ownedDevices.Any(c => c.Device.Equals(d)))
                .Cast<ClientDeviceBase>())
            {
                var owner = ownedDevices.First(o => o.Device.Equals(device));
                if (device.HasOwner(@this.Id, owner.Active))
                {
                    affectedDevices.Add(device);
                }
            }

            foreach (var device in deviceList
                .Where(d => d.IsOwner(@this.Id) && !ownedDevices.Any(c => c.Device.Equals(d)))
                .Cast<ClientDeviceBase>())
            {
                if (device.HasOwner(Constants.EgmHostId, device.Active))
                {
                    affectedDevices.Add(device);
                }
            }

            return affectedDevices;
        }

        /// <summary>
        ///     Assigns or removes this host as the configurator.
        /// </summary>
        /// <param name="this">The host.</param>
        /// <param name="devices">The list of available devices.</param>
        /// <param name="config">The list of devices for which this host is the configurator.</param>
        /// <returns>a list of affected devices.</returns>
        internal static IEnumerable<IDevice> Configures(
            this IHost @this,
            IEnumerable<IDevice> devices,
            IEnumerable<IDevice> config)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (devices == null)
            {
                throw new ArgumentNullException(nameof(devices));
            }

            var affectedDevices = new List<IDevice>();

            var deviceList = devices.ToList();
            var configList = config?.ToList() ?? Enumerable.Empty<IDevice>();

            foreach (var device in deviceList
                .Where(d => !d.IsConfigurator(@this.Id) && configList.Any(c => c.Equals(d)))
                .Cast<ClientDeviceBase>())
            {
                if (device.HasConfigurator(@this.Id))
                {
                    affectedDevices.Add(device);
                }
            }

            foreach (var device in deviceList
                .Where(d => d.IsConfigurator(@this.Id) && !configList.Any(c => c.Equals(d)))
                .Cast<ClientDeviceBase>())
            {
                if (device.HasConfigurator(Constants.EgmHostId))
                {
                    affectedDevices.Add(device);
                }
            }

            return affectedDevices;
        }

        /// <summary>
        ///     Assigns or removes this host as the configurator.
        /// </summary>
        /// <param name="this">The host.</param>
        /// <param name="devices">The list of available devices.</param>
        /// <param name="guest">The list of devices for which this host is the guest.</param>
        /// <returns>a list of affected devices.</returns>
        internal static IEnumerable<IDevice> GuestOf(
            this IHost @this,
            IEnumerable<IDevice> devices,
            IEnumerable<IDevice> guest)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (devices == null)
            {
                throw new ArgumentNullException(nameof(devices));
            }

            var affectedDevices = new List<IDevice>();

            var deviceList = devices.ToList();
            var guestList = guest?.ToList() ?? Enumerable.Empty<IDevice>();

            foreach (var device in deviceList.Where(d => !d.IsGuest(@this.Id) && guestList.Any(c => c.Equals(d)))
                .Cast<ClientDeviceBase>())
            {
                if (device.AddGuest(@this.Id))
                {
                    affectedDevices.Add(device);
                }
            }

            foreach (var device in deviceList.Where(d => d.IsGuest(@this.Id) && !guestList.Any(c => c.Equals(d)))
                .Cast<ClientDeviceBase>())
            {
                if (device.RemoveGuest(@this.Id))
                {
                    affectedDevices.Add(device);
                }
            }

            return affectedDevices;
        }
    }
}