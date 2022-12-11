namespace Aristocrat.Monaco.Gaming
{
    using ProtoBuf;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Runtime.Serialization;
    using Application.Contracts;
    using Application.Contracts.Drm;
    using Application.Contracts.Localization;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.VHD;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Contracts.Components;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     An <see cref="Kernel.Contracts.IInstaller" /> implementation for games
    /// </summary>
    public class GameInstaller : IGameInstaller, IPropertyProvider, IDisposable
    {
        private const string GamePackages = @"Game.Packages";
        private const string HashManifestExtension = @"hashes";
        private const int WaitTimeOut = 10000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IEventBus _bus;
        private readonly IDigitalRights _digitalRights;
        private readonly IComponentRegistry _componentRegistry;
        private readonly IPersistentStorageAccessor _gamePackagesAccessor;
        private readonly IGameProvider _games;
        private readonly IPropertiesManager _properties;

        private readonly ConcurrentDictionary<string, VirtualDiskHandle> _mounts =
            new ConcurrentDictionary<string, VirtualDiskHandle>();

        private readonly IPathMapper _pathMapper;
        private readonly IVirtualDisk _virtualDisk;

        private bool _disposed;
        private EventWaitHandle _uninstallEventWaitHandle;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameInstaller" /> class.
        /// </summary>
        public GameInstaller(
            IVirtualDisk virtualDisk,
            IPathMapper pathMapper,
            IGameProvider games,
            IPersistentStorageManager persistentStorage,
            IComponentRegistry componentRegistry,
            IEventBus bus,
            IDigitalRights digitalRights,
            IPropertiesManager properties)
        {
            _virtualDisk = virtualDisk ?? throw new ArgumentNullException(nameof(virtualDisk));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _games = games ?? throw new ArgumentNullException(nameof(games));
            if (persistentStorage == null)
            {
                throw new ArgumentNullException(nameof(persistentStorage));
            }
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _digitalRights = digitalRights ?? throw new ArgumentNullException(nameof(digitalRights));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            var blockName = GetType().ToString();

            _gamePackagesAccessor = persistentStorage.GetAccessor(BlockGamePackagesDataLevel, blockName);
        }

        private PersistenceLevel BlockGamePackagesDataLevel =>
            _properties.GetValue(ApplicationConstants.DemonstrationMode, false)
                ? PersistenceLevel.Critical
                : PersistenceLevel.Static;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameInstaller) };

        /// <inheritdoc />
        public bool DeviceChanged => true;

        /// <inheritdoc />
        public ExitAction? ExitAction => null;

        /// <inheritdoc />
        public event EventHandler UninstallStartedEventHandler;

        /// <inheritdoc />
        public void Initialize()
        {
            var packages = GetInstalledPackages();

            foreach (var package in packages)
            {
                if (AttachImage(package))
                {
                    _games.Register(package.MountPath);
                }
            }

            InstallNewGames(packages);

            RegisterHashManifest();
        }

        /// <inheritdoc />
        public bool Install(string packageId)
        {
            Logger.Debug($"Installing game {packageId}");

            var gamePackage = GetFileInfo(packageId);
            if (gamePackage == null)
            {
                Logger.Error(
                    $"Failed to locate an {GamingConstants.PackageExtension} file for package id: {packageId}");
                return false;
            }

            if (!_digitalRights.IsLicensed(gamePackage))
            {
                Logger.Warn($"Package is not licensed: {packageId}");

                return false;
            }

            var games = _pathMapper.GetDirectory(GamingConstants.GamesPath);

            var mountPath = Path.Combine(games.FullName, packageId);

            var virtualDisk = AttachImage(mountPath, gamePackage.FullName);
            if (virtualDisk.IsInvalid)
            {
                Logger.Error($"Failed to mount {gamePackage} at {mountPath}");

                return false;
            }

            var current = _games.Exists(mountPath);

            var currentFolder = current?.Folder;

            var package = GetInstalledPackages().FirstOrDefault(
                i => i.MountPath.Equals(current?.Folder, StringComparison.InvariantCultureIgnoreCase));

            if (current == null ? !_games.Add(mountPath) : !_games.Replace(mountPath, current))
            {
                Logger.Error($"Failed to install game package {gamePackage.FullName}");

                _virtualDisk.DetachImage(virtualDisk, mountPath);
                virtualDisk.Dispose();

                SafeDirectoryDelete(mountPath);

                return false;
            }

            if (package != null)
            {
                Logger.Debug($"The package at {currentFolder} will be removed");

                Uninstall(package.PackageId, false);

                Logger.Info($"Package {package.PackageId} was uninstalled");

                // The file must be deleted to ensure it doesn't get processed again
                RemovePackage(package.PackageId);
            }

            AddPackage(packageId, gamePackage.FullName, mountPath);

            if (current == null)
            {
                Logger.Info($"Installed game {packageId} from {gamePackage.FullName}");
                _bus.Publish(new GameInstalledEvent(packageId, gamePackage.FullName));
                _bus.Publish(new MediaAlteredEvent(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaType),
                                                   Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InstallReason),
                                                   packageId));
            }
            else
            {
                Logger.Info($"Upgraded game {packageId} from {gamePackage.FullName}");
                _bus.Publish(new GameUpgradedEvent(packageId, gamePackage.FullName));
                _bus.Publish(new MediaAlteredEvent(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaType),
                                                   Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UpgradeReason),
                                                   packageId));
            }

            return true;
        }

        /// <inheritdoc />
        public bool Uninstall(string packageId)
        {
            return Uninstall(packageId, true);
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>(GamingConstants.GamePackages, GetInstalledPackages())
        };

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case GamingConstants.GamePackages:
                    return GetInstalledPackages();
                default:
                    var errorMessage = "Unknown game installer property: " + propertyName;
                    Logger.Error(errorMessage);
                    throw new UnknownPropertyException(errorMessage);
            }
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            // No external sets for this provider...
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
                Logger.Debug("Cleaning up game folder mounts");

                if (_uninstallEventWaitHandle != null)
                {
                    _uninstallEventWaitHandle.Set();
                    _uninstallEventWaitHandle.Dispose();
                }

                foreach (var mount in _mounts)
                {
                    _virtualDisk.DetachImage(mount.Value, mount.Key);
                    SafeDirectoryDelete(mount.Key);
                    mount.Value.Dispose();

                    Logger.Debug($"Unmounted folder {mount.Key}");
                }

                _mounts.Clear();
                Logger.Info("Game folders have been unmounted");
            }

            _uninstallEventWaitHandle = null;

            _disposed = true;
        }

        private static byte[] ToByteArray(IReadOnlyCollection<InstalledPackage> packages)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, packages);

                return stream.ToArray();
            }
        }

        private static void SafeDirectoryDelete(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private bool Uninstall(string packageId, bool removeGame)
        {
            var signaled = true;

            if (UninstallStartedEventHandler != null)
            {
                _uninstallEventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                UninstallStartedEventHandler.Invoke(this, new InstallerEventArgs(_uninstallEventWaitHandle));
                signaled = _uninstallEventWaitHandle.WaitOne(WaitTimeOut);
                _uninstallEventWaitHandle.Dispose();
                _uninstallEventWaitHandle = null;
            }

            return signaled && UninstallInternal(packageId, removeGame);
        }

        private bool UninstallInternal(string packageId, bool removeGame)
        {
            Logger.Debug($"Uninstalling game {packageId}");

            var packages = GetInstalledPackages();

            var installedPackage = packages.FirstOrDefault(p => p.PackageId == packageId);
            if (installedPackage == null)
            {
                Logger.Error($"Package {packageId} is not part of the installed packages");
                return false;
            }

            if (_disposed)
            {
                return false;
            }

            if (removeGame)
            {
                _games.Remove(installedPackage.MountPath);
            }

            if (_mounts.TryRemove(installedPackage.MountPath, out var handle))
            {
                Logger.Info($"The mounted path {installedPackage.MountPath} will be unmounted");
                _virtualDisk.DetachImage(handle, installedPackage.MountPath);
                handle.Dispose();
            }

            SafeDirectoryDelete(installedPackage.MountPath);

            packages.Remove(installedPackage);

            _gamePackagesAccessor[GamePackages] = ToByteArray(packages);

            Logger.Info($"Uninstalled game {packageId}");

            if (removeGame)
            {
                _bus.Publish(new GameUninstalledEvent(packageId, installedPackage.Package));
                _bus.Publish(new MediaAlteredEvent(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaType),
                                                   Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UninstallReason),
                                                   packageId));
            }

            var file = new FileInfo(installedPackage.Package);
            _componentRegistry.UnRegister(file.Name);

            return true;
        }

        private bool AttachImage(InstalledPackage package)
        {
            if (!_digitalRights.IsLicensed(new FileInfo(package.Package)))
            {
                Logger.Warn($"Package is not licensed: {package.PackageId}");

                return false;
            }

            var handle = AttachImage(package.MountPath, package.Package);

            if (handle == null || handle.IsInvalid)
            {
                Uninstall(package.PackageId);
                return false;
            }

            return true;
        }

        private VirtualDiskHandle AttachImage(string mountPath, string gamePackage)
        {
            Logger.Debug($"Preparing to mount game package {gamePackage} at {mountPath}");

            if (_mounts.TryRemove(mountPath, out var current))
            {
                Logger.Info($"The mount path {mountPath} already exists and will be unmounted");
                _virtualDisk.DetachImage(current, mountPath);
                current.Dispose();
            }

            SafeDirectoryDelete(mountPath);

            Directory.CreateDirectory(mountPath);

            var handle = _virtualDisk.AttachImage(gamePackage, mountPath);
            if (!handle.IsInvalid)
            {
                if (!_mounts.TryAdd(mountPath, handle))
                {
                    handle.Close();
                    handle.SetHandleAsInvalid();
                }
                else
                {
                    RegisterComponent(gamePackage);
                }
            }

            Logger.Info($"Mounted game package {gamePackage} at {mountPath}");

            return handle;
        }

        private void AddPackage(string packageId, string package, string mountPath)
        {
            var packages = GetInstalledPackages();

            packages.Add(
                new InstalledPackage
                {
                    PackageId = packageId,
                    Package = package,
                    MountPath = mountPath
                });

            _gamePackagesAccessor[GamePackages] = ToByteArray(packages);
        }

        private List<InstalledPackage> GetInstalledPackages()
        {
            var rawPackages = (byte[])_gamePackagesAccessor[GamePackages];
            if (rawPackages.Length > 2)
            {
                using (var stream = new MemoryStream())
                {
                    stream.Write(rawPackages, 0, rawPackages.Length);
                    stream.Position = 0;
                    
                    return Serializer.Deserialize<List<InstalledPackage>>(stream);
                }
            }

            return Enumerable.Empty<InstalledPackage>().ToList();
        }

        private void InstallNewGames(IEnumerable<InstalledPackage> installed)
        {
            var packagePath = _pathMapper.GetDirectory(GamingConstants.PackagesPath);

            var unmounted = Directory.EnumerateFiles(packagePath.FullName, $"*.{GamingConstants.PackageExtension}")
                .Where(
                    i => installed.All(p => p.Package != i) &&
                         i.IndexOf(
                             ApplicationConstants.JurisdictionPackagePrefix,
                             StringComparison.InvariantCultureIgnoreCase) == -1 &&
                         i.IndexOf(
                             GamingConstants.PlatformPackagePrefix,
                             StringComparison.InvariantCultureIgnoreCase) == -1 &&
                         i.IndexOf(
                             GamingConstants.RuntimePackagePrefix,
                             StringComparison.InvariantCultureIgnoreCase) == -1);

            foreach (var file in unmounted)
            {
                Install(Path.GetFileNameWithoutExtension(file));
            }
        }

        private void RegisterHashManifest()
        {
            var path = _pathMapper.GetDirectory(GamingConstants.PackagesPath);

            var hashManifest = Directory.EnumerateFiles(path.FullName, $"*.{HashManifestExtension}").FirstOrDefault();

            // The hash manifest is optional.  If it's not present we can simply bail out.  The launcher checks for multiple - skipping that here
            if (string.IsNullOrEmpty(hashManifest))
            {
                return;
            }

            Logger.Info($"Discovered and registering hash manifest - {hashManifest}");

            RegisterComponent(hashManifest, ComponentType.Software);
        }

        private void RegisterComponent(string file, ComponentType type = ComponentType.Module)
        {
            var package = new FileInfo(file);

            _componentRegistry.Register(
                new Component
                {
                    ComponentId = package.Name,
                    Description = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GamePackageDescription),
                    Path = package.FullName,
                    Size = package.Length,
                    FileSystemType = FileSystemType.File,
                    Type = type
                });
        }

        private FileInfo GetFileInfo(string packageId)
        {
            var packages = _pathMapper.GetDirectory(GamingConstants.PackagesPath);

            var gamePackage = packages.GetFiles(
                    $"{packageId}.{GamingConstants.PackageExtension}",
                    SearchOption.TopDirectoryOnly)
                .FirstOrDefault();
            return gamePackage;
        }

        private void RemovePackage(string packageId)
        {
            var package = GetFileInfo(packageId);
            if (package?.DirectoryName != null)
            {
                var files = Directory.EnumerateFiles(package.DirectoryName, Path.ChangeExtension(package.Name, "*"));
                foreach (var file in files)
                {
                    File.Delete(file);
                }

                Logger.Info($"Game package {package.FullName} has been removed");
            }
        }

        [ProtoContract]
        private class InstalledPackage : IInstalledPackage
        {
            [ProtoMember(1)]
            public string MountPath { get; set; }

            [ProtoMember(2)]
            public string PackageId { get; set; }

            [ProtoMember(3)]
            public string Package { get; set; }
        }
    }
}