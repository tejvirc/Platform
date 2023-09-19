namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Media;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Emdi;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Common.CertificateManager;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Kernel;
    using log4net;
    using Meters;
    using Services;

    /// <summary>
    ///     The G2S Engine.  Responsible for starting the G2S client.
    /// </summary>
    public class G2SEngine : IEngine
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICertificateMonitor _certificateMonitor;
        private readonly ICertificateService _certificateService;
        private readonly IDeviceFactory _deviceFactory;
        private readonly IDeviceRegistryService _deviceRegistryService;
        private readonly IDeviceObserver _deviceStateObserver;
        private readonly IProgressiveDeviceManager _progressiveDeviceManager;
        private readonly IG2SEgm _egm;
        private readonly IEgmStateObserver _egmStateObserver;
        private readonly IEmdi _emdi;
        private readonly ICentralService _central;
        private readonly IG2SMeterProvider _g2SMeterProvider;
        private readonly IGatComponentFactory _gatComponentFactory;
        private readonly IHostFactory _hostFactory;
        private readonly IMasterResetService _masterResetService;
        private readonly IMetersSubscriptionManager _metersSubscriptionManager;
        private readonly IPropertiesManager _properties;
        private readonly IScriptManager _scriptManager;
        private readonly IPackageDownloadManager _packageDownloadManager;
        private readonly ISelfTest _selfTest;
        private readonly IVoucherDataService _voucherDataService;
        private readonly IEventLift _eventLift;
        private readonly IMultiProtocolConfigurationProvider _multiProtocolConfigurationProvider;

        private bool _disposed;

        public G2SEngine(
            IG2SEgm egm,
            IPropertiesManager properties,
            IHostFactory hostFactory,
            IDeviceFactory deviceFactory,
            IScriptManager scriptManager,
            IPackageDownloadManager packageDownloadManager,
            IDeviceObserver deviceStateObserver,
            IProgressiveDeviceManager progressiveDeviceManager,
            IEgmStateObserver egmStateObserver,
            IDeviceRegistryService deviceRegistryService,
            IGatComponentFactory gatComponentFactory,
            IMetersSubscriptionManager metersSubscriptionManager,
            IG2SMeterProvider game2SMeterProvider,
            IVoucherDataService voucherDataService,
            IMasterResetService masterResetService,
            ISelfTest selfTest,
            ICertificateService certificateService,
            ICertificateMonitor certificateMonitor,
            IEmdi emdi,
            ICentralService central,
            IEventLift eventLift,
            IMultiProtocolConfigurationProvider multiProtocolConfigurationProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _hostFactory = hostFactory ?? throw new ArgumentNullException(nameof(hostFactory));
            _deviceFactory = deviceFactory ?? throw new ArgumentNullException(nameof(deviceFactory));
            _scriptManager = scriptManager ?? throw new ArgumentNullException(nameof(scriptManager));
            _packageDownloadManager = packageDownloadManager ??
                                      throw new ArgumentNullException(nameof(packageDownloadManager));
            _deviceStateObserver = deviceStateObserver ?? throw new ArgumentNullException(nameof(deviceStateObserver));
            _progressiveDeviceManager = progressiveDeviceManager ?? throw new ArgumentNullException(nameof(progressiveDeviceManager));
            _egmStateObserver = egmStateObserver ?? throw new ArgumentNullException(nameof(egmStateObserver));
            _deviceRegistryService = deviceRegistryService ?? throw new ArgumentNullException(nameof(deviceRegistryService));
            _gatComponentFactory = gatComponentFactory ?? throw new ArgumentNullException(nameof(gatComponentFactory));
            _metersSubscriptionManager = metersSubscriptionManager ??
                                         throw new ArgumentNullException(nameof(metersSubscriptionManager));
            _g2SMeterProvider = game2SMeterProvider ?? throw new ArgumentNullException(nameof(game2SMeterProvider));
            _voucherDataService = voucherDataService ?? throw new ArgumentNullException(nameof(voucherDataService));
            _masterResetService = masterResetService ?? throw new ArgumentNullException(nameof(masterResetService));
            _selfTest = selfTest ?? throw new ArgumentNullException(nameof(selfTest));
            _certificateService = certificateService ?? throw new ArgumentNullException(nameof(certificateService));
            _certificateMonitor = certificateMonitor ?? throw new ArgumentNullException(nameof(certificateMonitor));
            _emdi = emdi ?? throw new ArgumentNullException(nameof(emdi));
            _central = central ?? throw new ArgumentNullException(nameof(central));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _multiProtocolConfigurationProvider = multiProtocolConfigurationProvider ?? throw new ArgumentNullException(nameof(multiProtocolConfigurationProvider));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Start(IStartupContext context, Action onReady = null)
        {
            LoadConfiguration();

            _selfTest.Execute();
            _voucherDataService.Start();
            SetHostEnabled();

            onReady?.Invoke();

            _gatComponentFactory.RegisterComponents();

            // Don't like doing this here, but we can't enable/start comms if the cert it invalid when SCEP is enabled
            // TODO: Refactor this to be driven (i.e. requested) by the G2S framework
            if (!_certificateService.IsCertificateManagementEnabled() || _certificateService.HasValidCertificate())
            {
                _egm.Start(context.ContextPerHost(_egm.Hosts));
            }
            else
            {
                Logger.Warn("The G2S client service has started without starting comms due to an invalid certificate");
            }

            _g2SMeterProvider.Start();
            _scriptManager.Start();
            _packageDownloadManager.Start();
            _metersSubscriptionManager.Start();
            _masterResetService.Start();
            _certificateMonitor.Start();
            _egmStateObserver.Subscribe();
            _central.Start();

            Logger.Info("The G2S client service has started");
        }

        /// <inheritdoc />
        public void Stop()
        {
            _egmStateObserver.Unsubscribe();

            _egm.Stop();

            TeardownEgm();

            _emdi.Stop();

            Logger.Info("The G2S client service host has closed.");
        }

        /// <inheritdoc />
        public void Restart(IStartupContext context)
        {
            Stop();

            Start(context);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                TeardownEgm();
            }

            _disposed = true;
        }

        private void LoadConfiguration()
        {
            var protocol = _multiProtocolConfigurationProvider.MultiProtocolConfiguration
                    .Single(c => c.Protocol == CommsProtocol.G2S);


            var hosts = _properties.GetValues<IHost>(Constants.RegisteredHosts).ToList();
            foreach (var host in hosts.Where(h => h.Registered))
            {
                _hostFactory.Create(host);
            }

            // assign appropriate hosts for each protocol capability scenario
            var registeredGuests = hosts.Where(h => !h.IsEgm() && h.Registered).ToList();
            var defaultHost = hosts.OrderBy(h => h.Index).FirstOrDefault(h => !h.IsEgm() && h.Registered && !h.IsProgressiveHost);
            var validationHost = protocol.IsValidationHandled ? defaultHost : null;
            var progressiveHost = protocol.IsProgressiveHandled ? hosts.FirstOrDefault(h => h.IsProgressiveHost) : null;
            var egmHost = _egm.GetHostById(Aristocrat.G2S.Client.Constants.EgmHostId) as IHost;

            // Create and register the non-host oriented devices
            // The host that will own various devices will change
            // based on the current multi-protocol configuration
            _deviceFactory.Create(validationHost ?? progressiveHost ?? egmHost, registeredGuests,
                () => new CabinetDevice(_deviceStateObserver, _egmStateObserver));
            _deviceFactory.Create(validationHost ?? progressiveHost ?? egmHost, registeredGuests,
                () => new CommConfigDevice(_deviceStateObserver));
            _deviceFactory.Create(validationHost ?? progressiveHost ?? egmHost, registeredGuests,
                () => new DownloadDevice(
                    _deviceStateObserver,
                    !_properties.GetValue(ApplicationConstants.ReadOnlyMediaRequired, false)));
            _deviceFactory.Create(defaultHost ?? egmHost, registeredGuests,
                () => new CoinAcceptorDevice(_deviceStateObserver));
            _deviceFactory.Create(defaultHost ?? egmHost,
                () => new AuditMetersDevice(_deviceStateObserver));

            var printer = _deviceRegistryService.GetDevice<IPrinter>();
            if (printer != null)
            {
                _deviceFactory.Create(defaultHost ?? egmHost, registeredGuests,
                    () => new PrinterDevice(printer.PrinterId, _deviceStateObserver));
            }

            var noteAcceptor = _deviceRegistryService.GetDevice<INoteAcceptor>();
            if (noteAcceptor != null)
            {
                _deviceFactory.Create(
                    defaultHost ?? egmHost, registeredGuests,
                    () => new NoteAcceptorDevice(1, _deviceStateObserver));
            }

            var idProvider = _deviceRegistryService.GetDevice<IIdReaderProvider>();
            if (idProvider != null)
            {
                foreach (var reader in idProvider.Adapters)
                {
                    _deviceFactory.Create(defaultHost ?? egmHost, registeredGuests,
                        () => new IdReaderDevice(reader.IdReaderId, _deviceStateObserver));
                }
            }

            var idReaders = _egm.GetDevices<IIdReaderDevice>().ToList();
            var player = _deviceFactory.Create(
                defaultHost ?? egmHost, registeredGuests,
                () => new PlayerDevice(1, _deviceStateObserver, idReaders, _eventLift));

            _deviceFactory.Create(defaultHost ?? egmHost, registeredGuests,
                () => new InformedPlayerDevice(1, _deviceStateObserver, _eventLift) { Player = player as IPlayerDevice });

            _deviceFactory.Create(defaultHost ?? egmHost, registeredGuests,
                () => new ChooserDevice(1, _deviceStateObserver));

            foreach (var mediaPlayer in _properties.GetValues<IMediaPlayer>(ApplicationConstants.MediaPlayers))
            {
                _deviceFactory.Create(defaultHost ?? egmHost, registeredGuests,
                    () => new MediaDisplayDevice(mediaPlayer.Id, _deviceStateObserver));

                _emdi.Start(mediaPlayer.Port);
            }

            var games = _properties.GetValues<IGameDetail>(GamingConstants.Games).ToList();

            foreach (var game in games)
            {
                _deviceFactory.Create(validationHost ?? egmHost, registeredGuests,
                    () => new GamePlayDevice(game.Id, _deviceStateObserver));
            }

            if (games.Any(g => g.CentralAllowed))
            {
                _deviceFactory.Create(defaultHost ?? egmHost, registeredGuests,
                    () => new CentralDevice(_deviceStateObserver));
            }

            _deviceFactory.Create(defaultHost ?? egmHost, registeredGuests,
                () => new HandpayDevice(1, _deviceStateObserver));

            _deviceFactory.Create(defaultHost ?? egmHost, registeredGuests,
                () => new StorageDevice(_deviceStateObserver));

            _deviceFactory.Create(defaultHost ?? egmHost, registeredGuests,
                () => new BonusDevice(1, _deviceStateObserver));

            if (protocol.IsValidationHandled)
            {
                _deviceFactory.Create(validationHost ?? egmHost,
                    () => new VoucherDevice(_deviceStateObserver, _eventLift));
            }
            
            if (protocol.IsProgressiveHandled)
            {
                _progressiveDeviceManager.AddProgressiveDevices();
            }

            _deviceFactory.Create(
                defaultHost ?? egmHost,
                registeredGuests,
                () => new AnalyticsDevice(1, _deviceStateObserver));
        }

        private void SetHostEnabled()
        {
            var cabinetDevice = _egm.GetDevice<ICabinetDevice>();

            foreach (var device in _egm.Devices)
            {
                if (device.Existing && device is IRestartStatus status)
                {
                    if (cabinetDevice.RestartStatusMode && !status.RestartStatus)
                    {
                        device.HostEnabled = false;
                    }
                    else if (!cabinetDevice.RestartStatusMode)
                    {
                        device.HostEnabled = status.RestartStatus;
                    }
                }
                else if (device.Owner == Aristocrat.G2S.Client.Constants.EgmHostId)
                {
                    device.HostEnabled = true;
                }
                else if (!device.HostEnabled)
                {
                    _deviceStateObserver.Notify(device, nameof(device.HostEnabled));
                }
            }
        }

        private void TeardownEgm()
        {
            foreach (var device in _egm.Devices)
            {
                var removed = _egm.RemoveDevice(device) as IDisposable;
                removed?.Dispose();
            }
        }
    }
}
