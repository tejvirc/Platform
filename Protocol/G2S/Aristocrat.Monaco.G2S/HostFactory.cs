namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Linq;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;

    /// <summary>
    ///     An implementation of IHostFactory
    /// </summary>
    public class HostFactory : IHostFactory
    {
        private readonly ICommunicationsStateObserver _commsStateObserver;
        private readonly IDeviceFactory _deviceFactory;
        private readonly IDeviceObserver _deviceStateObserver;
        private readonly IG2SEgm _egm;
        private readonly IEventPersistenceManager _eventPersistenceManager;
        private readonly ITransportStateObserver _transportStateObserver;
        private readonly IEventLift _eventLift;
        private readonly IEgmStateManager _egmStateManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostFactory" /> class.
        /// </summary>
        /// <param name="egm">The EGM</param>
        /// <param name="deviceFactory">The device factory.</param>
        /// <param name="transportStateObserver">The transport state observer.</param>
        /// <param name="commsStateObserver">The communication state observer.</param>
        /// <param name="deviceStateObserver">The device state observer.</param>
        /// <param name="eventPersistenceManager">The event persistence manager.</param>
        /// <param name="eventLift">The event lift.</param>
        /// <param name="egmStateManager">An instance of IEgmStateManager.</param>
        public HostFactory(
            IG2SEgm egm,
            IDeviceFactory deviceFactory,
            ITransportStateObserver transportStateObserver,
            ICommunicationsStateObserver commsStateObserver,
            IDeviceObserver deviceStateObserver,
            IEventPersistenceManager eventPersistenceManager,
            IEventLift eventLift,
            IEgmStateManager egmStateManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transportStateObserver =
                transportStateObserver ?? throw new ArgumentNullException(nameof(transportStateObserver));
            _commsStateObserver = commsStateObserver ?? throw new ArgumentNullException(nameof(commsStateObserver));
            _deviceStateObserver = deviceStateObserver ?? throw new ArgumentNullException(nameof(deviceStateObserver));
            _deviceFactory = deviceFactory ?? throw new ArgumentNullException(nameof(deviceFactory));
            _eventPersistenceManager =
                eventPersistenceManager ?? throw new ArgumentNullException(nameof(eventPersistenceManager));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _egmStateManager = egmStateManager ?? throw new ArgumentNullException(nameof(egmStateManager));
        }

        /// <inheritdoc />
        public IHostControl Create(IHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (host.IsEgm())
            {
                return _egm.Hosts.Single(h => h.IsEgm());
            }

            var registeredHost = _egm.RegisterHost(host.Id, host.Address, host.RequiredForPlay, host.Index);

            RegisterHostOrientedDevices(registeredHost);

            return registeredHost;
        }

        /// <inheritdoc />
        public IHostControl Update(IHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (host.IsEgm())
            {
                return _egm.Hosts.Single(h => h.IsEgm());
            }

            var registeredHost = _egm.Hosts.Single(h => h.Index == host.Index);

            registeredHost.SetAddress(host.Address);
            registeredHost.SetRequiredForPlay(host.RequiredForPlay);

            return registeredHost;
        }

        /// <inheritdoc />
        public void Delete(IHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (host.IsEgm())
            {
                return;
            }

            _egm.UnregisterHost(host.Id, _egmStateManager);
        }

        private void RegisterHostOrientedDevices(IHost host)
        {
            _deviceFactory.Create(
                host,
                () => new CommunicationsDevice(
                    host.Id,
                    _deviceStateObserver,
                    _egm.Address,
                    host.RequiredForPlay,
                    _transportStateObserver,
                    _commsStateObserver,
                    _eventLift));
            _deviceFactory.Create(
                host,
                () => new EventHandlerDevice(host.Id, _deviceStateObserver, _eventPersistenceManager, _eventLift));
            _deviceFactory.Create(host, () => new GatDevice(host.Id, _deviceStateObserver));
            _deviceFactory.Create(host, () => new OptionConfigDevice(host.Id, _deviceStateObserver));
            _deviceFactory.Create(host, () => new MetersDevice(host.Id, _deviceStateObserver));
        }
    }
}