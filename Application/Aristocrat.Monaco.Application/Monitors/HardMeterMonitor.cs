namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts;
    using Hardware.Contracts.HardMeter;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Definition of the HardMeterMonitor class
    /// </summary>
    public class HardMeterMonitor : IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Guid HardMeterDisabled = ApplicationConstants.HardMeterDisabled;

        private readonly IEventBus _bus;
        private readonly IHardMeter _hardMeters;
        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _properties;

        private bool _disposed;

        public HardMeterMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IHardMeter>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public HardMeterMonitor(IHardMeter hardMeters, ISystemDisableManager disableManager, IPropertiesManager properties, IEventBus bus)
        {
            _hardMeters = hardMeters ?? throw new ArgumentNullException(nameof(hardMeters));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { GetType() };

        /// <inheritdoc />
        public void Initialize()
        {
            var configuration = ConfigurationUtilities.GetConfiguration(
                ApplicationConstants.JurisdictionConfigurationExtensionPath,
                () => new ApplicationConfiguration
                {
                    HardMeterMonitor = new ApplicationConfigurationHardMeterMonitor { DisableOnError = false }
                });

            if (!(configuration?.HardMeterMonitor?.DisableOnError ?? false) ||
                !_properties.GetValue(HardwareConstants.HardMetersEnabledKey, true))
            {
                return;
            }

            Subscribe();

            if (!_hardMeters.Enabled)
            {
                DisableSystem();
            }

            Logger.Info("Initialized");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void Subscribe()
        {
            _bus.Subscribe<DisabledEvent>(this, _ => DisableSystem());

            _bus.Subscribe<EnabledEvent>(this, _ => EnableSystem());
        }

        private void DisableSystem()
        {
            _disableManager.Disable(
                HardMeterDisabled,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardMeterDisabled));
        }

        private void EnableSystem()
        {
            _disableManager.Enable(HardMeterDisabled);
        }
    }
}