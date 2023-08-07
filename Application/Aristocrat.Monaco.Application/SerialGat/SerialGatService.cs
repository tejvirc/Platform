namespace Aristocrat.Monaco.Application.SerialGat
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Authentication;
    using Contracts.Localization;
    using Contracts.SerialGat;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class SerialGatService : ISerialGat, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IAuthenticationService _authenticationService;
        private readonly IComponentRegistry _componentRegistry;
        private readonly Guid _disableGuid = Guid.NewGuid();

        private readonly IEventBus _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
        private readonly IPersistentStorageManager _storageManager;
        private readonly ISystemDisableManager _systemDisableManager;

        private bool _disposed;

        private SerialGatApplicationLayer _gatApplication;

        public SerialGatService(
            ISystemDisableManager systemDisableManager,
            IPersistentStorageManager storageManager,
            IAuthenticationService authenticationService,
            IComponentRegistry componentRegistry)
        {
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _authenticationService =
                authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
        }

        public ApplicationConfigurationGatSerial Config { get; private set; }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => GetType().Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(ISerialGat) };

        public bool IsConnected => _gatApplication?.Connected ?? false;

        public void Initialize()
        {
            Task.Run(
                () =>
                {
                    Config = new ApplicationConfigurationGatSerial();

                    var configuration = ConfigurationUtilities.GetConfiguration(
                        ApplicationConstants.JurisdictionConfigurationExtensionPath,
                        () => new ApplicationConfiguration { GatSerial = Config });

                    if (configuration.GatSerial != null)
                    {
                        Config = configuration.GatSerial;
                    }

                    StartGat();

                    _eventBus.Subscribe<SerialGatVersionChangedEvent>(this, HandleVersionChanged);
                });
        }

        public string GetStatus()
        {
            if (_gatApplication == null)
            {
                return Localizer.ForLockup().GetString(ResourceKeys.GatDisconnected);
            }

            var statusMessage = _gatApplication.Connected ?
                Localizer.ForLockup().GetString(ResourceKeys.GatConnected) :
                Localizer.ForLockup().GetString(ResourceKeys.GatDisconnected);

            switch (_gatApplication.CalculationStatus)
            {
                case SerialGatCalculationStatus.Idle:
                    statusMessage += Localizer.ForLockup().GetString(ResourceKeys.GatIdle);
                    break;
                case SerialGatCalculationStatus.AuthenticatingAll:
                    statusMessage += Localizer.ForLockup().GetString(ResourceKeys.GatVerifyingAll);
                    break;
                case SerialGatCalculationStatus.AuthenticatingComponent:
                    statusMessage += string.Format(
                        Localizer.ForLockup().GetString(ResourceKeys.GatVerifyingSingleTemplate),
                        _gatApplication.AuthenticatingComponentName);
                    break;
            }

            return statusMessage;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                StopGat();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void HandleVersionChanged(SerialGatVersionChangedEvent theEvent)
        {
            if (theEvent.Version != Config.Version)
            {
                Task.Run(
                    () =>
                    {
                        StopGat();

                        Config.Version = theEvent.Version;

                        StartGat();
                    });
            }
        }

        private void StartGat()
        {
            try
            {
                _gatApplication = new SerialGatApplicationLayer(
                    _eventBus,
                    _storageManager,
                    _authenticationService,
                    _componentRegistry,
                    Config);
                _gatApplication.ConnectionStatusChanged += HandleGatStatusEvent;
                _gatApplication.Enable();
            }
            catch (ArgumentOutOfRangeException)
            {
                Logger.Error($"Port {Config.ComPort} can not be found or is already in use");
                _gatApplication.Disable();
                _gatApplication.Dispose();
                _gatApplication = null;
            }
        }

        private void StopGat()
        {
            if (_gatApplication != null)
            {
                _gatApplication.ConnectionStatusChanged -= HandleGatStatusEvent;
                _gatApplication.Disable();
                _gatApplication.Dispose();
                _gatApplication = null;
            }

            _systemDisableManager.Enable(_disableGuid);
        }

        private void HandleGatStatusEvent(object sender, EventArgs args)
        {
            if (_gatApplication.Connected)
            {
                // Get status right now, rather than having disable manager check later when it's too late
                var status = GetStatus();
                _systemDisableManager.Disable(_disableGuid, SystemDisablePriority.Immediate, () => status);
            }
            else
            {
                _systemDisableManager.Enable(_disableGuid);
            }

            _eventBus.Publish(new SerialGatStatusEvent(GetStatus()));
        }
    }
}