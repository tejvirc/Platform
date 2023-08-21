namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Contracts.Components;
    using Localization.Properties;
    using Runtime;

    public class SoftwareInstaller : ISoftwareInstaller
    {
        private const string RuntimeWildcardSearch =
            GamingConstants.RuntimePackagePrefix + @"*." + GamingConstants.PackageExtension;

        private const string PlatformWildcardSearch =
            GamingConstants.PlatformPackagePrefix + @"*." + GamingConstants.PackageExtension;

        private const string JurisdictionWildcardSearch =
            ApplicationConstants.JurisdictionPackagePrefix + @"*." + GamingConstants.PackageExtension;

        private const string IndexField = @"BlockIndex";
        private const string FileNameField = @"FileName";
        private const string ActiveField = @"Active";
        private const int MaxStoredMedia = 100;

        private readonly IComponentRegistry _components;

        private readonly string _dataBlockName;
        private readonly IEventBus _eventBus;
        private readonly string _indexBlockName;
        private readonly IPathMapper _pathMapper;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IRuntimeProvider _runtimeLoader;

        private IPersistentStorageAccessor _indexAccessor;
        private IPersistentStorageAccessor _mediaHistoryAccessor;

        public SoftwareInstaller(
            IPathMapper pathMapper,
            IComponentRegistry components,
            IRuntimeProvider runtimeLoader,
            IEventBus eventBus,
            IPersistentStorageManager persistentStorageManager
        )
        {
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _components = components ?? throw new ArgumentNullException(nameof(components));
            _runtimeLoader = runtimeLoader ?? throw new ArgumentNullException(nameof(runtimeLoader));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _persistentStorage = persistentStorageManager ?? throw new ArgumentNullException(nameof(persistentStorageManager));

            _indexBlockName = $"{typeof(SoftwareInstaller)}.Index";
            _dataBlockName = $"{typeof(SoftwareInstaller)}.Data";
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(ISoftwareInstaller) };

        public bool DeviceChanged => true;

        public ExitAction? ExitAction => Kernel.Contracts.ExitAction.ShutDown;

        /// <inheritdoc />
        public event EventHandler UninstallStartedEventHandler;

        public void Initialize()
        {
            CreatePersistence();

            // This is not the best fit for this, but the game installer handles the game component registration so it's probably OK
            RegisterComponents();
        }

        public bool Install(string packageId)
        {
            if (IsRuntime(packageId))
            {
                _eventBus.Publish(
                    new MediaAlteredEvent(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypeRuntime),
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InstallReason),
                        packageId));
                return true;
            }

            if (IsPlatform(packageId))
            {
                _eventBus.Publish(
                    new MediaAlteredEvent(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypePlatform),
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InstallReason),
                        packageId));
                return true;
            }

            if (IsJurisdiction(packageId))
            {
                _eventBus.Publish(
                    new MediaAlteredEvent(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypeJurisdiction),
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InstallReason),
                        packageId));
                return true;
            }

            return false;
        }

        public bool Uninstall(string packageId)
        {
            UninstallStartedEventHandler?.Invoke(this, new EventArgs());

            if (IsRuntime(packageId))
            {
                _runtimeLoader.Unload(packageId);

                _eventBus.Publish(
                    new MediaAlteredEvent(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypeRuntime),
                        string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UninstallReason), packageId),
                        packageId));
                return true;
            }

            if (IsPlatform(packageId))
            {
                _eventBus.Publish(
                    new MediaAlteredEvent(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypePlatform),
                        string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UninstallReason), packageId),
                        packageId));
                return true;
            }

            if (IsJurisdiction(packageId))
            {
                _eventBus.Publish(
                    new MediaAlteredEvent(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypeJurisdiction),
                        string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UninstallReason), packageId),
                        packageId));
            }

            return false;
        }

        private static bool IsRuntime(string packageId)
        {
            return packageId.StartsWith(
                GamingConstants.RuntimePackagePrefix,
                StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsPlatform(string packageId)
        {
            return packageId.StartsWith(
                GamingConstants.PlatformPackagePrefix,
                StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsJurisdiction(string packageId)
        {
            return packageId.StartsWith(
                ApplicationConstants.JurisdictionPackagePrefix,
                StringComparison.InvariantCultureIgnoreCase);
        }

        private void RegisterComponents()
        {
            Register(RuntimeWildcardSearch, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RuntimePackageDescription));

            Register(PlatformWildcardSearch, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PlatformPackageDescription));

            Register(JurisdictionWildcardSearch, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.JurisdictionPackageDescription));

            CheckForRemoval();
        }

        private void Register(string searchPattern, string description)
        {
            var packages = _pathMapper.GetDirectory(GamingConstants.PackagesPath);

            var files = packages?.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);

            var isRuntime = IsRuntime(searchPattern);
            if (files is null)
            {
                return;
            }

            foreach (var file in files)
            {
                var fileName = file.Name;

                _components.Register(
                    new Component
                    {
                        ComponentId = fileName,
                        Description = description,
                        Path = file.FullName,
                        Size = file.Length,
                        FileSystemType = FileSystemType.File,
                        Type = ComponentType.Module
                    });

                if (ReportEvent(fileName, isRuntime ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypeRuntime)
                                                    : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypePlatform)))
                {
                    StoreComponentAlteration(fileName);
                }
            }
        }

        private void CheckForRemoval()
        {
            for (var index = 0; index < MaxStoredMedia; ++index)
            {
                var fileName = (string)_mediaHistoryAccessor[index, FileNameField];
                if (string.IsNullOrEmpty(fileName))
                {
                    break;
                }

                var mediaIsActive = (bool)_mediaHistoryAccessor[index, ActiveField];
                if (!mediaIsActive)
                {
                    continue;
                }

                if (!File.Exists(Path.Combine(_pathMapper.GetDirectory(GamingConstants.PackagesPath).FullName, fileName)))
                {
                    //mark it as inactive as it is deleted  from block
                    UpdateBlock(index, false);

                    _eventBus.Publish(
                        new MediaAlteredEvent(
                            IsRuntime(fileName) ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypeRuntime)
                                                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MediaTypePlatform),
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.UninstallReason),
                            fileName));
                }
            }
        }

        private void UpdateBlock(int index, bool status)
        {
            using (var transaction = _mediaHistoryAccessor.StartTransaction())
            {
                transaction[_dataBlockName, index, ActiveField] = status;
                transaction.Commit();
            }
        }

        private bool ReportEvent(string fileName, string mediaType)
        {
            var index = FileStatus(fileName);
            if (index == -1)
            {
                _eventBus.Publish(
                    new MediaAlteredEvent(
                        mediaType,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InstallReason),
                        fileName));
                return true;
            }

            var status = (bool)_mediaHistoryAccessor[index, ActiveField];
            if (status)
            {
                return false;
            }

            _eventBus.Publish(new MediaAlteredEvent(mediaType, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InstallReason), fileName));

            UpdateBlock(index, true);

            return false;
        }

        private int FileStatus(string fileName)
        {
            var index = -1; //File does not exist in storage

            for (var i = 0; i < MaxStoredMedia; ++i)
            {
                var file = (string)_mediaHistoryAccessor[i, FileNameField];
                if (string.IsNullOrEmpty(file))
                {
                    break;
                }

                if (!file.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                index = i; //File is already stored in storage
                break;
            }

            return index;
        }

        private void StoreComponentAlteration(string fileName)
        {
            var index = ((int)_indexAccessor[IndexField] + 1) % MaxStoredMedia;

            using (var transaction = _mediaHistoryAccessor.StartTransaction())
            {
                transaction.AddBlock(_indexAccessor);
                transaction[_indexBlockName, IndexField] = index;
                transaction[_dataBlockName, index, FileNameField] = fileName;
                transaction[_dataBlockName, index, ActiveField] = true;
                transaction.Commit();
            }
        }

        private void CreatePersistence()
        {
            if (_persistentStorage.BlockExists(_indexBlockName))
            {
                _indexAccessor = _persistentStorage.GetBlock(_indexBlockName);
            }
            else
            {
                // Create and init to -1, since the initial log entry will increment it making the first entry 0
                _indexAccessor = _persistentStorage.CreateBlock(PersistenceLevel.Critical, _indexBlockName, 1);
                _indexAccessor[IndexField] = -1;
            }

            if (_persistentStorage.BlockExists(_dataBlockName))
            {
                _mediaHistoryAccessor = _persistentStorage.GetBlock(_dataBlockName);
            }
            else
            {
                _mediaHistoryAccessor = _persistentStorage.CreateBlock(
                    PersistenceLevel.Critical,
                    _dataBlockName,
                    MaxStoredMedia);
            }
        }
    }
}