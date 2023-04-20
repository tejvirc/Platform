namespace Aristocrat.Monaco.Hardware.Services
{
    using Aristocrat.Monaco.Kernel.Contracts;
    using Contracts;
    using Contracts.Dfu;
    using Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using Properties;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     An <see cref="IInstaller" /> implementation for firmware
    /// </summary>
    public abstract class FirmwareInstallerBase : IInstaller
    {
        private const string DownloadsTempPath = "/Downloads/temp";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IComponentRegistry _componentRegistry;
        private readonly IEventBus _eventBus;
        private readonly IDfuProvider _dfuProvider;
        private readonly IPathMapper _pathMapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FirmwareInstallerBase" /> class.
        /// </summary>
        protected FirmwareInstallerBase(
            DeviceType deviceType,
            string path,
            IPathMapper pathMapper,
            IComponentRegistry componentRegistry,
            IEventBus eventBus,
            IDfuProvider dfuProvider)
        {
            DeviceType = deviceType;
            Path = path;
            _pathMapper = pathMapper;
            _componentRegistry = componentRegistry;
            _eventBus = eventBus;
            _dfuProvider = dfuProvider;
        }

        /// <inheritdoc />
        public bool DeviceChanged => true;

        /// <inheritdoc />
        public ExitAction? ExitAction => null;

        /// <inheritdoc />
        public event EventHandler UninstallStartedEventHandler;

        private DeviceType DeviceType { get; }

        public string Path { get; }

        /// <inheritdoc />
        public bool Install(string packageId)
        {
            var packages = _pathMapper.GetDirectory(DownloadsTempPath);
            Logger.Debug($"Installing firmware {packageId}");
            var firmwarePackage = packages.GetFiles($"{packageId}.dfu", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (firmwarePackage == null)
            {
                Logger.Error($"Failed to locate dfu file for package id: {packageId}");
                return false;
            }

            if (DeviceType.Equals(default(DeviceType)))
            {
                Logger.Error("Invalid installer type.");
                return false;
            }

            var components = _componentRegistry?.Components;

            var component = components?.FirstOrDefault(a => a.Path == Path);

            _eventBus.Publish(new DfuDownloadStartEvent(DeviceType));

            if (!_dfuProvider.Download(firmwarePackage.FullName).Result)
            {
                _eventBus.Publish(new DfuErrorEvent());
                return false;
            }

            // remove old component
            if (component != null && _componentRegistry?.Components.Count(a => a.Path == component.Path) > 1)
            {
                _componentRegistry?.UnRegister(component.ComponentId);
            }

            Logger.Info($"Installed firmware {packageId} from {firmwarePackage.FullName}");
            _eventBus.Publish(new FirmwareInstalledEvent(packageId, DeviceType));
            _eventBus.Publish(new MediaAlteredEvent(Resources.MediaTypeFirmware, Resources.InstallReason, packageId));
            return true;
        }

        /// <inheritdoc />
        public bool Uninstall(string packageId)
        {
            //TODO: implement uninstall
            OnUninstall();
            _eventBus.Publish(new MediaAlteredEvent(Resources.MediaTypeFirmware, Resources.UninstallReason, packageId));
            return true;
        }

        private void OnUninstall()
        {
            UninstallStartedEventHandler?.Invoke(this, EventArgs.Empty);
        }
    }
}
