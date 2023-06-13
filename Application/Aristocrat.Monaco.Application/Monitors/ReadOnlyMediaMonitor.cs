namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;

    public class ReadOnlyMediaMonitor : IService
    {
        private const string TempFile = "test.tmp";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _bus;
        private readonly ISystemDisableManager _disableManager;
        private readonly IPathMapper _pathMapper;
        private readonly IPropertiesManager _properties;

        public ReadOnlyMediaMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public ReadOnlyMediaMonitor(
            IPropertiesManager properties,
            IPathMapper pathMapper,
            ISystemDisableManager disableManager,
            IEventBus bus)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public string Name => nameof(ReadOnlyMediaMonitor);

        public ICollection<Type> ServiceTypes => new[] { typeof(ReadOnlyMediaMonitor) };

        public void Initialize()
        {
            if (!_properties.GetValue(ApplicationConstants.ReadOnlyMediaRequired, false))
            {
                return;
            }

#if !(RETAIL)
            if (_properties.GetValue(@"readonlymediaoptional", "false") == "true")
            {
                return;
            }
#endif

            var manifestPath = _pathMapper.GetDirectory(ApplicationConstants.ManifestPath);
            var tempFile = Path.Combine(manifestPath.FullName, TempFile);

            try
            {
                File.WriteAllText(tempFile, string.Empty);

                File.Delete(tempFile);
            }
            catch (IOException e)
            {
                Logger.Info($"Read only media active: {e.Message}");
                return;
            }

            var message = Localizer.DynamicCulture().GetString(ResourceKeys.ReadOnlyMediaFault);

            _disableManager.Disable(
                ApplicationConstants.ReadOnlyMediaDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.DynamicCulture().GetString(ResourceKeys.ReadOnlyMediaFault));

            _bus.Publish(new ReadOnlyMediaErrorEvent(message));

            Logger.Error("Read only media is not configured properly");
        }
    }
}