namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.IO;
    using System.Linq;
    using Aristocrat.G2S.Protocol.v21;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Common.PackageManager;
    using Common.PackageManager.Storage;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Kernel;
    using Kernel.Contracts.Components;
    using Localization.Properties;
    using Monaco.Common.Storage;
    using ComponentsConstants = Kernel.Contracts.Components.Constants;

    /// <summary>
    ///     An <see cref="IGatComponentFactory"/> implementation
    /// </summary>
    public class GatComponentFactory : IGatComponentFactory
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IModuleRepository _moduleRepository;
        private readonly IComponentRegistry _componentRegistry;
        private readonly IPackageManager _packages;
        private readonly IPropertiesManager _properties;

        public GatComponentFactory(
            IComponentRegistry componentRegistry,
            IPackageManager packages,
            IPropertiesManager properties,
            IModuleRepository moduleRepository,
            IMonacoContextFactory contextFactory)
        {
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _packages = packages ?? throw new ArgumentNullException(nameof(packages));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(moduleRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public void RegisterComponents()
        {
            foreach (var gamePackage in _properties.GetValues<IInstalledPackage>(GamingConstants.GamePackages))
            {
                RegisterGame(gamePackage.PackageId, gamePackage.Package);
            }

            var components = _componentRegistry.Components.Where(c => c.Type == ComponentType.Module).ToList();

            foreach (var platformPackages in components.Where(
                c => c.ComponentId.StartsWith(GamingConstants.PlatformPackagePrefix, StringComparison.InvariantCultureIgnoreCase)))
            {
                Register(ComponentType.Module, platformPackages.Path, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PlatformPackageDescription));
            }

            foreach (var jurisdictionPackages in components.Where(
                c => c.ComponentId.StartsWith(GamingConstants.JurisdictionPackagePrefix, StringComparison.InvariantCultureIgnoreCase)))
            {
                Register(ComponentType.Module, jurisdictionPackages.Path, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.JurisdictionPackageDescription));
            }

            foreach (var runtimePackages in components.Where(
                c => c.ComponentId.StartsWith(GamingConstants.RuntimePackagePrefix, StringComparison.InvariantCultureIgnoreCase)))
            {
                Register(ComponentType.Module, runtimePackages.Path, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RuntimePackageDescription));
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var modules = _moduleRepository.GetAll(context).ToList();

                var orphaned = modules.Where(c => components.All(r => r.ComponentId != c.ModuleId));
                foreach (var module in orphaned)
                {
                    _moduleRepository.Delete(context, module);
                }
            }

            components = _componentRegistry.Components.Where(c => c.Type == ComponentType.Hardware
                && (c.Path == ComponentsConstants.PrinterPath
                || c.Path == ComponentsConstants.NoteAcceptorPath) ).ToList();

            foreach(var peripheral in components)
            {
                AddModule(peripheral.ComponentId, string.Empty, t_modTypes.G2S_firmware);
            }

            components = _componentRegistry.Components.Where(
                c => c.Type == ComponentType.OS && c.Path == HardwareConstants.OperatingSystemPath).ToList();

            foreach (var os in components)
            {
                AddModule(os.ComponentId, string.Empty, t_modTypes.G2S_os);
            }
        }

        /// <inheritdoc />
        public void RegisterGame(string packageId, string package)
        {
            var fileInfo = new FileInfo(package);
            if (fileInfo.Exists)
            {
                var componentId = fileInfo.Name;

                AddModule(componentId, Path.GetFileNameWithoutExtension(fileInfo.Name), t_modTypes.G2S_game);
            }
        }

        public void Register(ComponentType componentType, string package, string description = null)
        {
            var fileInfo = new FileInfo(package);
            var componentId = fileInfo.Name;

            var entity = _packages.GetModuleEntity(componentId);
            if (entity == null)
            {
                entity = new Module
                {
                    ModuleId = componentId,
                    PackageId = Path.GetFileNameWithoutExtension(fileInfo.Name),
                    Status = _packages.ToXml(
                        new moduleStatus
                        {
                            modType = t_modTypes.G2S_other,
                            modStatus = t_modStates.G2S_enabled,
                            modId = componentId
                        })
                };

                _packages.UpdateModuleEntity(entity);
            }
        }

        private void AddModule(string componentId, string packageId, t_modTypes modType)
        {
            var entity = _packages.GetModuleEntity(componentId);
            if (entity == null)
            {
                entity = new Module
                {
                    ModuleId = componentId,
                    PackageId = packageId,
                    Status = _packages.ToXml(
                        new moduleStatus
                        {
                            modType = modType,
                            modStatus = t_modStates.G2S_enabled,
                            modId = componentId
                        })
                };

                _packages.UpdateModuleEntity(entity);
            }
        }
    }
}
