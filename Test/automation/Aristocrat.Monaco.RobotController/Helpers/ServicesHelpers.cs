namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using SimpleInjector;
    using System;
    using Aristocrat.Monaco.RobotController.Services;
    internal static class ServicesHelpers
    {
        internal static Container InitializeContainer(Container container, object context = null)
        {
            if (container == null)
            {
                throw new ArgumentException($"{nameof(container)} is null.");
            }

            if (context != null)
            {
                container.RegisterInstance(context.GetType(), context);
            }

            var serviceManager = ServiceManager.GetInstance();

            container.RegisterInstance(serviceManager.GetService<IEventBus>());
            container.RegisterInstance(serviceManager.GetService<IPropertiesManager>());

            var platformContainer = serviceManager.GetService<IContainerService>().Container;
            if(platformContainer == null)
            {
                throw new NullReferenceException($"{nameof(IContainerService.Container)} not available.");
            }

            container.RegisterInstance(platformContainer.GetInstance<ILobbyStateManager>());
            container.RegisterInstance(platformContainer.GetInstance<IGamePlayState>());
            container.RegisterInstance(platformContainer.GetInstance<IGameProvider>());
            container.RegisterInstance(platformContainer.GetInstance<IBank>());
            container.RegisterInstance(platformContainer.GetInstance<IPathMapper>());
            container.RegisterInstance(platformContainer.GetInstance<IGameService>());

            container.Register<RobotLogger>(Lifestyle.Singleton);
            container.Register<Automation>(Lifestyle.Singleton);
            container.Register<LobbyStateChecker>(Lifestyle.Singleton);
            container.Register<GamingService>(Lifestyle.Singleton);
            container.Register<RobotService>(Lifestyle.Singleton);
            container.Register<StatusManager>(Lifestyle.Singleton);

            container.Register<CashoutOperations>(Lifestyle.Transient);
            container.Register<GameOperations>(Lifestyle.Transient);
            container.Register<PlayerOperations>(Lifestyle.Transient);
            container.Register<TouchOperations>(Lifestyle.Transient);
            container.Register<LockUpOperations>(Lifestyle.Transient);
            container.Register<OperatingHoursOperations>(Lifestyle.Transient);
            container.Register<GameHelpOperations>(Lifestyle.Transient);
            container.Register<ServiceRequestOperations>(Lifestyle.Transient);
            container.Register<BalanceOperations>(Lifestyle.Transient);
            container.Register<RebootRequestOperations>(Lifestyle.Transient);
            container.Register<AuditMenuOperations>(Lifestyle.Transient);

            return container;
        }
    }
}
