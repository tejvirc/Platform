namespace Aristocrat.Monaco.Mgam.CompositionRoot
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Authentication;
    using Application.Contracts.Identification;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Aristocrat.Monaco.Protocol.Common.Installer;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Session;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Kernel.Contracts.Components;
    using Services.Event;
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
        /// <param name="container">The container.</param>
        /// <returns><see cref="Container" />.</returns>
        internal static Container RegisterExternalServices(this Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            var serviceManager = ServiceManager.GetInstance();

            container.RegisterInstance(serviceManager.GetService<IEventBus>());
            container.RegisterInstance(serviceManager.GetService<StartupEventListener>());
            container.RegisterInstance(serviceManager.GetService<IIO>());
            container.RegisterInstance(serviceManager.GetService<IPathMapper>());
            container.RegisterInstance(serviceManager.GetService<IPersistentStorageManager>());
            container.RegisterInstance(serviceManager.GetService<IPropertiesManager>());
            container.RegisterInstance(serviceManager.GetService<IPersistenceProvider>());
            container.RegisterInstance(serviceManager.GetService<ISystemDisableManager>());
            container.RegisterInstance(serviceManager.GetService<ILocalization>());
            container.RegisterInstance(serviceManager.GetService<IDoorService>());
            container.RegisterInstance(serviceManager.GetService<IGameProvider>());
            container.RegisterInstance(serviceManager.GetService<ITransactionCoordinator>());
            container.RegisterInstance(serviceManager.GetService<ICabinetService>());
            container.RegisterInstance(serviceManager.GetService<IGameHistory>());
            container.RegisterInstance(serviceManager.GetService<IBank>());
            container.RegisterInstance(serviceManager.GetService<IPlayerBank>());
            container.RegisterInstance(serviceManager.GetService<IPlayerSessionHistory>());
            container.RegisterInstance(serviceManager.GetService<IAttendantService>());
            container.RegisterInstance(serviceManager.GetService<IOperatorMenuLauncher>());
            container.RegisterInstance(serviceManager.GetService<IIdProvider>());
            container.RegisterInstance(serviceManager.GetService<IIdentificationProvider>());
            container.RegisterInstance(serviceManager.GetService<ICentralProvider>());
            container.RegisterInstance(serviceManager.GetService<IMeterManager>());
            container.RegisterInstance(serviceManager.GetService<IAuthenticationService>());
            container.RegisterInstance(serviceManager.GetService<IEmployeeLogin>());
            container.RegisterInstance(serviceManager.GetService<IComponentRegistry>());
            container.RegisterInstance(serviceManager.GetService<IAudio>());
            container.RegisterInstance(serviceManager.GetService<ITiltLogger>());
            container.RegisterInstance(serviceManager.GetService<ITransactionHistory>());
            container.RegisterInstance(serviceManager.GetService<IGamePlayState>());
            container.RegisterInstance(serviceManager.GetService<ITowerLight>());
            container.RegisterInstance(serviceManager.GetService<IAutoPlayStatusProvider>());
            container.RegisterInstance(serviceManager.GetService<ITime>());
            container.RegisterInstance(serviceManager.GetService<IProtocolLinkedProgressiveAdapter>());
            container.RegisterInstance(serviceManager.GetService<IProtocolProgressiveEventsRegistry>());
            container.RegisterInstance<IInstallerFactory>(
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

            return container;
        }
    }
}