namespace Aristocrat.Monaco.G2S.CompositionRoot
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Authentication;
    using Application.Contracts.Localization;
    using Application.Contracts.Media;
    using Protocol.Common.Installer;
    using Kernel.Contracts.MessageDisplay;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Session;
    using Hardware.Contracts;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.HardMeter;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Kernel.Contracts.Components;
    using Monaco.Common.Storage;
    using SimpleInjector;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Handles configuring the services managed by the ServiceManager.
    /// </summary>
    internal static class ExternalServiceBuilder
    {
        /// <summary>
        ///     Registers services managed outside this assembly.
        /// </summary>
        /// <param name="this">The container.</param>
        internal static void RegisterExternalServices(this Container @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var serviceManager = ServiceManager.GetInstance();

            @this.RegisterInstance(serviceManager.GetService<IMonacoContextFactory>());
            @this.RegisterInstance(serviceManager.GetService<IEventBus>());
            @this.RegisterInstance(serviceManager.GetService<IPathMapper>());
            @this.RegisterInstance(serviceManager.GetService<IPersistentStorageManager>());
            @this.RegisterInstance(serviceManager.GetService<IPropertiesManager>());
            @this.RegisterInstance(serviceManager.GetService<IMessageDisplay>());
            @this.RegisterInstance(serviceManager.GetService<ITime>());
            @this.RegisterInstance(serviceManager.GetService<IDoorService>());
            @this.RegisterInstance(serviceManager.GetService<IHardMeter>());
            @this.RegisterInstance(serviceManager.GetService<IMeterManager>());
            @this.RegisterInstance(serviceManager.GetService<IGameMeterManager>());
            @this.RegisterInstance(serviceManager.GetService<ICabinetService>());
            @this.RegisterInstance(serviceManager.GetService<ISystemDisableManager>());
            @this.RegisterInstance(serviceManager.GetService<IBank>());
            @this.RegisterInstance(serviceManager.GetService<IGamePlayState>());
            @this.RegisterInstance(serviceManager.GetService<IDeviceRegistryService>());
            @this.RegisterInstance(serviceManager.GetService<ITransactionHistory>());
            @this.RegisterInstance(serviceManager.GetService<IGameProvider>());
            @this.RegisterInstance(serviceManager.GetService<IGameHistory>());
            @this.RegisterInstance(serviceManager.GetService<IPlayerBank>());
            @this.RegisterInstance(serviceManager.GetService<IIdProvider>());
            @this.RegisterInstance(serviceManager.GetService<IDisableByOperatorManager>());
            @this.RegisterInstance(serviceManager.GetService<IMediaPlayerResizeManager>());
            @this.RegisterInstance(serviceManager.GetService<IMediaProvider>());
            @this.RegisterInstance(serviceManager.GetService<IGameOrderSettings>());
            @this.RegisterInstance(serviceManager.GetService<IDisplayService>());
            @this.RegisterInstance(serviceManager.GetService<ITransferOutHandler>());
            @this.RegisterInstance(serviceManager.GetService<IPlayerSessionHistory>());
            @this.RegisterInstance(serviceManager.GetService<IPlayerService>());
            @this.RegisterInstance(serviceManager.GetService<ITransactionCoordinator>());
            @this.RegisterInstance(serviceManager.GetService<IAttendantService>());
            @this.RegisterInstance(serviceManager.GetService<IIdReaderProvider>());
            @this.RegisterInstance(serviceManager.GetService<IComponentRegistry>());
            @this.RegisterInstance(serviceManager.GetService<IAuthenticationService>());
            @this.RegisterInstance(serviceManager.GetService<IBonusHandler>());
            @this.RegisterInstance(serviceManager.GetService<IOperatorMenuLauncher>());
            @this.RegisterInstance(serviceManager.GetService<ITowerLight>());
            @this.RegisterInstance(serviceManager.GetService<IPersistenceProvider>());
            @this.RegisterInstance(serviceManager.GetService<ILocalization>());
            @this.RegisterInstance(serviceManager.GetService<ICentralProvider>());
            @this.RegisterInstance(serviceManager.GetService<IProtocolLinkedProgressiveAdapter>());
            @this.RegisterInstance(serviceManager.GetService<IProgressiveLevelProvider>());
            @this.RegisterInstance(serviceManager.GetService<IOSInstaller>());
            @this.RegisterInstance(serviceManager.GetService<IPrinterFirmwareInstaller>());
            @this.RegisterInstance(serviceManager.GetService<INoteAcceptorFirmwareInstaller>());
            @this.RegisterInstance<IInstallerFactory>(
                new InstallerFactory
                {
                    { @"winUpdate", serviceManager.GetService<IOSInstaller>() },
                    { @"game", serviceManager.GetService<IGameInstaller>() },
                    { @"printer", serviceManager.GetService<IPrinterFirmwareInstaller>() },
                    { @"noteacceptor", serviceManager.GetService<INoteAcceptorFirmwareInstaller>() },
                    { @"platform", serviceManager.GetService<ISoftwareInstaller>() },
                    { @"jurisdiction", serviceManager.GetService<ISoftwareInstaller>() },
                    { @"runtime", serviceManager.GetService<ISoftwareInstaller>() }
                });
        }
    }
}
