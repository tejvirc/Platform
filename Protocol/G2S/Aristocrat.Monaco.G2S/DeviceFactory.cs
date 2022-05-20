namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Data.Profile;

    /// <summary>
    ///     An IDeviceFactory implementation
    /// </summary>
    public class DeviceFactory : IDeviceFactory
    {
        private readonly IG2SEgm _egm;
        private readonly IProfileService _profiles;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceFactory" /> class.
        /// </summary>
        /// <param name="egm">The EGM</param>
        /// <param name="profiles">The profile service.</param>
        public DeviceFactory(IG2SEgm egm, IProfileService profiles)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        }

        /// <inheritdoc />
        public IDevice Create(IHost host, Func<ClientDeviceBase> createDevice)
        {
            return Create(host, null, createDevice);
        }

        /// <inheritdoc />
        public IDevice Create(IHost host, IEnumerable<IHost> guests, Func<ClientDeviceBase> createDevice)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (createDevice == null)
            {
                throw new ArgumentNullException(nameof(createDevice));
            }

            var device = createDevice();
            if (device != null)
            {
                RegisterDevice(host, guests, device);
            }

            return device;
        }

        private void RegisterDevice(IHost host, IEnumerable<IHost> guests, ClientDeviceBase device)
        {
            if (!_profiles.Exists(device))
            {
                if (!device.IsHostOriented())
                {
                    device.Owner(host, _egm);

                    if (guests != null)
                    {
                        foreach (var guest in guests)
                        {
                            device.Invited(guest);
                        }
                    }
                }

                device.Configurator(host);

                _profiles.Save(device);

                _egm.AddDevice(device);
            }
            else
            {
                _egm.AddDevice(device);

                HydrateDevice(device);

                device.SetQueue(_egm);
            }
        }

        private void HydrateDevice(IDevice device)
        {
            _profiles.Populate(device);
        }
    }
}