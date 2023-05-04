namespace Aristocrat.Monaco.Gaming.CompositionRoot
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Drm;
    using Application.Contracts.EdgeLight;
    using Application.Contracts.Media;
    using Application.Contracts.Operations;
    using Application.Contracts.Protocol;
    using Application.Contracts.Settings;
    using Application.Contracts.TiltLogger;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Bell;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.TowerLight;
    using Hardware.Contracts.VHD;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Contracts.Components;
    using SimpleInjector;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Handles configuring the services managed by the ServiceManager.
    /// </summary>
    internal static class ExternalServiceBuilder
    {
        /// <summary>
        ///     Registers the package manager with the container.
        /// </summary>
        /// <param name="this">The container.</param>
        internal static void RegisterExternalServices(this Container @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var serviceManager = ServiceManager.GetInstance();

            @this.RegisterInstance(serviceManager.GetService<IEventBus>());
            @this.RegisterInstance(serviceManager.GetService<IMeterManager>());
            @this.RegisterInstance(serviceManager.GetService<IPathMapper>());
            @this.RegisterInstance(serviceManager.GetService<IPersistentStorageManager>());
            @this.RegisterInstance(serviceManager.GetService<IPropertiesManager>());
            @this.RegisterInstance(serviceManager.GetService<IButtonDeckDisplay>());
            @this.RegisterInstance(serviceManager.GetService<IBank>());
            @this.RegisterInstance(serviceManager.GetService<ISystemDisableManager>());
            @this.RegisterInstance(serviceManager.GetService<IMessageDisplay>());
            @this.RegisterInstance(serviceManager.GetService<ITransferOutHandler>());
            @this.RegisterInstance(serviceManager.GetService<ITransactionCoordinator>());
            @this.RegisterInstance(serviceManager.GetService<IButtonService>());
            @this.RegisterInstance(serviceManager.GetService<IOperatorMenuLauncher>());
            @this.RegisterInstance(serviceManager.GetService<IVirtualDisk>());
            @this.RegisterInstance(serviceManager.GetService<IIO>());
            @this.RegisterInstance(serviceManager.GetService<IIdProvider>());
            @this.RegisterInstance(serviceManager.GetService<ITransactionHistory>());
            @this.RegisterInstance(serviceManager.GetService<IAudio>());
            @this.RegisterInstance(serviceManager.GetService<IMediaPlayerResizeManager>());
            @this.RegisterInstance(serviceManager.GetService<IMediaProvider>());
            @this.RegisterInstance(serviceManager.GetService<IComponentRegistry>());
            @this.RegisterInstance(serviceManager.GetService<IInitializationProvider>());
            @this.RegisterInstance(serviceManager.GetService<IIdReaderProvider>());
            @this.RegisterInstance(serviceManager.GetService<IConfigurationUtility>());
            @this.RegisterInstance(serviceManager.GetService<IDoorService>());
            @this.RegisterInstance(serviceManager.GetService<ITowerLight>());
            @this.RegisterInstance(serviceManager.GetService<IOperatingHoursMonitor>());
            @this.RegisterInstance(serviceManager.GetService<ISessionInfoService>());
            @this.RegisterInstance(serviceManager.GetService<IPersistenceProvider>());
            @this.RegisterInstance(serviceManager.GetService<ICabinetDetectionService>());
            @this.RegisterInstance(serviceManager.GetService<IEdgeLightingStateManager>());
            @this.RegisterInstance(serviceManager.GetService<IEdgeLightingController>());
            @this.RegisterInstance(serviceManager.GetService<IConfigurationSettingsManager>());
            @this.RegisterInstance(serviceManager.GetService<ITiltLogger>());
            @this.RegisterInstance(serviceManager.GetService<IDigitalRights>());
            @this.RegisterInstance(serviceManager.GetService<IMultiProtocolConfigurationProvider>());
            @this.RegisterInstance(serviceManager.GetService<IDisplayService>());
            @this.RegisterInstance(serviceManager.GetService<IBell>());
            @this.RegisterInstance(serviceManager.GetService<IBeagleBoneController>());
            @this.RegisterInstance(serviceManager.GetService<ITime>());
            @this.RegisterInstance(serviceManager.GetService<IMoneyLaunderingMonitor>());
            @this.RegisterInstance(serviceManager.GetService<IGpuDetailService>());
        }
    }
}