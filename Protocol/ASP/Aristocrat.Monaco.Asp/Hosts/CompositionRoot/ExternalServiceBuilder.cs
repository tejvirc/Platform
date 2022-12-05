namespace Aristocrat.Monaco.Asp.Hosts.CompositionRoot
{
    using System;
    using Application.Contracts;
    using Application.Contracts.OperatorMenu;
    using Kernel.Contracts.MessageDisplay;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Progressives;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Kernel;
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
            @this.RegisterInstance(serviceManager.GetService<IPropertiesManager>());
            @this.RegisterInstance(serviceManager.GetService<IGameProvider>());
            @this.RegisterInstance(serviceManager.GetService<IPersistentStorageManager>());

            @this.RegisterInstance(serviceManager.GetService<IProtocolProgressiveEventsRegistry>());
            @this.RegisterInstance(serviceManager.GetService<IProtocolLinkedProgressiveAdapter>());
            @this.RegisterInstance(serviceManager.GetService<IProgressiveMeterManager>());
            @this.RegisterInstance(serviceManager.GetService<IProgressiveLevelProvider>());

            //@this.RegisterInstance(serviceManager.GetService<IPropertiesManager>());
            @this.RegisterInstance(serviceManager.GetService<IMessageDisplay>());
            //@this.RegisterInstance(serviceManager.GetService<ITime>());
            @this.RegisterInstance(serviceManager.GetService<IDoorService>());
            //@this.RegisterInstance(serviceManager.GetService<INVRam>());
            //@this.RegisterInstance(serviceManager.GetService<IKeySwitch>());
            @this.RegisterInstance(serviceManager.GetService<IGameMeterManager>());
            @this.RegisterInstance(serviceManager.GetService<ICabinetService>());
            @this.RegisterInstance(serviceManager.GetService<ISystemDisableManager>());
            //@this.RegisterInstance(serviceManager.GetService<IBank>());
            @this.RegisterInstance(serviceManager.GetService<IGamePlayState>());
            //@this.RegisterInstance(serviceManager.GetService<IDeviceRegistryService>());
            //@this.RegisterInstance(serviceManager.GetService<ITransactionHistory>());
            @this.RegisterInstance(serviceManager.GetService<IGameHistory>());
            //@this.RegisterInstance(serviceManager.GetService<IGameService>());
            //@this.RegisterInstance(serviceManager.GetService<IPlayerBank>());
            //@this.RegisterInstance(serviceManager.GetService<IIdProvider>());
            //@this.RegisterInstance(serviceManager.GetService<IMediaProvider>());
            //@this.RegisterInstance(serviceManager.GetService<IGameOrderSettings>());
            //@this.RegisterInstance(serviceManager.GetService<IDisplayService>());
            //@this.RegisterInstance(serviceManager.GetService<ITransferOutHandler>());
            //@this.RegisterInstance(serviceManager.GetService<IPlayerSessionHistory>());
            //@this.RegisterInstance(serviceManager.GetService<IPlayerService>());
            //@this.RegisterInstance(serviceManager.GetService<ITransactionCoordinator>());
            //@this.RegisterInstance(serviceManager.GetService<IIdReaderProvider>());
            //@this.RegisterInstance(serviceManager.GetService<ILight>());
            @this.RegisterInstance(serviceManager.GetService<IDoorMonitor>());
            @this.RegisterInstance(serviceManager.GetService<IOperatorMenuLauncher>());
            @this.RegisterInstance(serviceManager.GetService<IOperatorMenuGamePlayMonitor>());
            @this.RegisterInstance(serviceManager.GetService<IDisableByOperatorManager>());
            //@this.RegisterInstance(serviceManager.GetService<IWatTransferOnHandler>());
            //@this.RegisterInstance(serviceManager.GetService<IWatOffProvider>());
            //@this.RegisterInstance(serviceManager.GetService<IAuthenticationService>());
            //@this.RegisterInstance(serviceManager.GetService<IAudio>());
            @this.RegisterInstance(serviceManager.GetService<IBonusHandler>());
            //@this.RegisterInstance(serviceManager.GetService<IComponentRegistry>());
            // TODO: no service available => @this.RegisterSingleton(serviceManager.GetService<IJackpotStrategy>());
            @this.RegisterInstance(serviceManager.GetService<IFundsTransferDisable>());
        }
    }
}