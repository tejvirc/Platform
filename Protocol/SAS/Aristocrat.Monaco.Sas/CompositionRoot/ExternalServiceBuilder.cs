namespace Aristocrat.Monaco.Sas.CompositionRoot
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Application.Contracts.Authentication;
    using Application.Contracts.Media;
    using Application.Contracts.Protocol;
    using Kernel.Contracts.MessageDisplay;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Configuration;
    using Gaming.Contracts.Lobby;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Session;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.KeySwitch;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.SerialPorts;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Kernel.Contracts.Components;
    using SimpleInjector;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     This class is mainly for configuring the services managed by the ServiceManager.
    /// </summary>
    internal static class ExternalServiceBuilder
    {
        /// <summary>
        ///     Registers services managed outside this assembly.
        /// </summary>
        /// <param name="container">The container.</param>
        internal static Container RegisterExternalServices(this Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            var serviceManager = ServiceManager.GetInstance();

            container.RegisterInstance(serviceManager.GetService<IEventBus>());
            container.RegisterInstance(serviceManager.GetService<IPathMapper>());
            container.RegisterInstance(serviceManager.GetService<IPersistentStorageManager>());
            container.RegisterInstance(serviceManager.GetService<IPropertiesManager>());
            container.RegisterInstance(serviceManager.GetService<IMessageDisplay>());
            container.RegisterInstance(serviceManager.GetService<ITime>());
            container.RegisterInstance(serviceManager.GetService<IDoorService>());
            container.RegisterInstance(serviceManager.GetService<IKeySwitch>());
            container.RegisterInstance(serviceManager.GetService<IMeterManager>());
            container.RegisterInstance(serviceManager.GetService<IGameMeterManager>());
            container.RegisterInstance(serviceManager.GetService<IProgressiveMeterManager>());
            container.RegisterInstance(serviceManager.GetService<ICabinetService>());
            container.RegisterInstance(serviceManager.GetService<ISystemDisableManager>());
            container.RegisterInstance(serviceManager.GetService<IBank>());
            container.RegisterInstance(serviceManager.GetService<IGamePlayState>());
            container.RegisterInstance(serviceManager.GetService<IDeviceRegistryService>());
            container.RegisterInstance(serviceManager.GetService<ITransactionHistory>());
            container.RegisterInstance(serviceManager.GetService<IGameProvider>());
            container.RegisterInstance(serviceManager.GetService<IGameHistory>());
            container.RegisterInstance(serviceManager.GetService<IGameService>());
            container.RegisterInstance(serviceManager.GetService<IPlayerBank>());
            container.RegisterInstance(serviceManager.GetService<IIdProvider>());
            container.RegisterInstance(serviceManager.GetService<IDisableByOperatorManager>());
            container.RegisterInstance(serviceManager.GetService<IMediaProvider>());
            container.RegisterInstance(serviceManager.GetService<IGameOrderSettings>());
            container.RegisterInstance(serviceManager.GetService<IDisplayService>());
            container.RegisterInstance(serviceManager.GetService<ITransferOutHandler>());
            container.RegisterInstance(serviceManager.GetService<IPlayerSessionHistory>());
            container.RegisterInstance(serviceManager.GetService<IPlayerService>());
            container.RegisterInstance(serviceManager.GetService<ITransactionCoordinator>());
            container.RegisterInstance(serviceManager.GetService<IIdReaderProvider>());
            container.RegisterInstance(serviceManager.GetService<ITowerLight>());
            container.RegisterInstance(serviceManager.GetService<IDoorMonitor>());
            container.RegisterInstance(serviceManager.GetService<IOperatorMenuLauncher>());
            container.RegisterInstance(serviceManager.GetService<IWatTransferOnHandler>());
            container.RegisterInstance(serviceManager.GetService<IWatOffProvider>());
            container.RegisterInstance(serviceManager.GetService<IAuthenticationService>());
            container.RegisterInstance(serviceManager.GetService<IAudio>());
            container.RegisterInstance(serviceManager.GetService<IBonusHandler>());
            container.RegisterInstance(serviceManager.GetService<IComponentRegistry>());
            container.RegisterInstance(serviceManager.GetService<IPersistenceProvider>());
            container.RegisterInstance(serviceManager.GetService<IComponentMonitor>());
            container.RegisterInstance(serviceManager.GetService<IButtonLamps>());
            container.RegisterInstance(serviceManager.GetService<ISerialPortsService>());
            container.RegisterInstance(serviceManager.GetService<IFundsTransferDisable>());
            container.RegisterInstance(serviceManager.GetService<IAutoPlayStatusProvider>());
            container.RegisterInstance(serviceManager.GetService<IProtocolLinkedProgressiveAdapter>());
            container.RegisterInstance(serviceManager.GetService<IMultiProtocolConfigurationProvider>());
            container.RegisterInstance(serviceManager.GetService<IProgressiveLevelProvider>()); 
            container.RegisterInstance(serviceManager.GetService<IContainerService>().Container.GetInstance<IRuntimeFlagHandler>());
            container.RegisterInstance(serviceManager.GetService<IMoneyLaunderingMonitor>());
            container.RegisterInstance(serviceManager.GetService<IContainerService>().Container.GetInstance<ILobbyStateManager>());
            container.RegisterInstance(serviceManager.GetService<IConfigurationProvider>());
            return container;
        }
    }
}