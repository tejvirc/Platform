namespace Aristocrat.Monaco.G2S
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Data.Profile;
    using IDevice = Aristocrat.G2S.Client.Devices.IDevice;

    /// <summary>
    ///     An implementation of an <see cref="IDeviceObserver" />
    /// </summary>
    public class DeviceObserver : IDeviceObserver
    {
        private readonly IHostStatusHandlerFactory _hostStatusHandlerFactory;
        private readonly IEgmStateManager _egmState;
        private readonly IProfileService _profiles;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceObserver" /> class.
        /// </summary>
        public DeviceObserver(
            IProfileService profiles,
            IEgmStateManager egmState,
            IHostStatusHandlerFactory hostStatusHandlerFactory)
        {
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _egmState = egmState ?? throw new ArgumentNullException(nameof(egmState));
            _hostStatusHandlerFactory = hostStatusHandlerFactory ?? throw new ArgumentNullException(nameof(hostStatusHandlerFactory));
        }

        /// <inheritdoc cref="IDeviceObserver.Notify" />
        public void Notify(IDevice device, string propertyName)
        {
            switch (propertyName)
            {
                case nameof(device.Enabled):
                    HandleStatusChange(device, EgmState.EgmDisabled, device.Enabled);
                    break;
                case nameof(device.HostEnabled):
                    _profiles.Save(device);

                    HandleHostStatusChange(device, EgmState.HostDisabled, device.HostEnabled);
                    break;
                default:
                    _profiles.Save(device);
                    break;
            }
        }

        private void HandleHostStatusChange(IDevice device, EgmState state, bool enable)
        {
            var handler = _hostStatusHandlerFactory.Create(device) as dynamic;

            handler?.Handle(device as dynamic);

            HandleStatusChange(device, state, enable);
        }

        private void HandleStatusChange(IDevice device, EgmState state, bool enable)
        {
            // Skip this for certain devices:
            //  The cabinet is handled within the device
            //  The others represent physical devices that are disabled by the system.
            if (state == EgmState.EgmDisabled &&
                (device is ICabinetDevice || device is INoteAcceptorDevice || device is IPrinterDevice ||
                 device is IProgressiveDevice))
            {
                return;
            }

            if (enable)
            {
                _egmState.Enable(device, state);
            }
            else if (device.RequiredForPlay)
            {
                _egmState.Disable(device, state, false, () => device.DisableText, false);
            }
        }
    }
}
