namespace Aristocrat.Monaco.Bingo.CompositionRoot
{
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Protocol;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Configuration;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Payment;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using SimpleInjector;
    using Vgt.Client12.Application.OperatorMenu;

    public static class ExternalServiceBuilder
    {
        public static Container AddExternalServices(this Container container)
        {
            var serviceManager = ServiceManager.GetInstance();
            container.RegisterInstance(serviceManager.GetService<IEventBus>());
            container.RegisterInstance(serviceManager.GetService<IGameProvider>());
            container.RegisterInstance(serviceManager.GetService<IMultiProtocolConfigurationProvider>());
            container.RegisterInstance(serviceManager.GetService<IPropertiesManager>());
            container.RegisterInstance(serviceManager.GetService<IPathMapper>());
            container.RegisterInstance(serviceManager.GetService<ICabinetDetectionService>());
            container.RegisterInstance(serviceManager.GetService<ISystemDisableManager>());
            container.RegisterInstance(serviceManager.GetService<ICentralProvider>());
            container.RegisterInstance(serviceManager.GetService<IMessageDisplay>());
            container.RegisterInstance(serviceManager.GetService<IGamePlayState>());
            container.RegisterInstance(serviceManager.GetService<ITransactionHistory>());
            container.RegisterInstance(serviceManager.GetService<IGameHistory>());
            container.RegisterInstance(serviceManager.GetService<IPaymentDeterminationProvider>());
            container.RegisterInstance(serviceManager.GetService<IIdProvider>());
            container.RegisterInstance(serviceManager.GetService<IGameMeterManager>());
            container.RegisterInstance(serviceManager.GetService<IPersistentStorageManager>());
            container.RegisterInstance(serviceManager.GetService<IBonusHandler>());
            container.RegisterInstance(serviceManager.GetService<IPlayerBank>());
            container.RegisterInstance(serviceManager.GetService<IMeterManager>());
            container.RegisterInstance(serviceManager.GetService<IOperatorMenuLauncher>());
            container.RegisterInstance(serviceManager.GetService<IDoorMonitor>());
            container.RegisterInstance(serviceManager.GetService<IGameDiagnostics>());
            container.RegisterInstance(serviceManager.GetService<IGameConfigurationProvider>());
            container.RegisterInstance(serviceManager.GetService<IConfigurationProvider>());
            container.RegisterInstance(serviceManager.GetService<IServerPaytableInstaller>());
            return container;
        }
    }
}
