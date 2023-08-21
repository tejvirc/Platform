namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Linq;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Gaming.Contracts;
    using Kernel;
    using Constants = G2S.Constants;

    /// <summary>
    ///     Handles the <see cref="GameAddedEvent" /> event.
    /// </summary>
    public class GameAddedConsumer : Consumes<GameAddedEvent>
    {
        private readonly IDeviceFactory _deviceFactory;
        private readonly IDeviceObserver _deviceObserver;
        private readonly IG2SEgm _egm;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameAddedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance</param>
        /// <param name="deviceFactory">An <see cref="IDeviceFactory" /> instance</param>
        /// <param name="deviceObserver">An <see cref="IDeviceObserver" /> instance</param>
        public GameAddedConsumer(
            IG2SEgm egm,
            IPropertiesManager properties,
            IDeviceFactory deviceFactory,
            IDeviceObserver deviceObserver)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _deviceFactory = deviceFactory ?? throw new ArgumentNullException(nameof(deviceFactory));
            _deviceObserver = deviceObserver ?? throw new ArgumentNullException(nameof(deviceObserver));
        }

        /// <inheritdoc />
        public override void Consume(GameAddedEvent theEvent)
        {
            var hosts = _properties.GetValues<IHost>(Constants.RegisteredHosts).ToList();

            // We're using the first host as the default if it exists otherwise the EGM
            var defaultHost = hosts.OrderBy(h => h.Index).FirstOrDefault(h => !h.IsEgm() && h.Registered);

            var device = new GamePlayDevice(theEvent.GameId, _deviceObserver);
            _deviceFactory.Create(
                defaultHost ?? _egm.GetHostById(Aristocrat.G2S.Client.Constants.EgmHostId),
                null,
                () => device);
            _deviceObserver.Notify(device, nameof(device.HostEnabled));
        }
    }
}