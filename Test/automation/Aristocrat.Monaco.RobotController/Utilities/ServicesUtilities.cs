namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using SimpleInjector;
    using System;

    internal static class ServicesUtilities
    {
        internal static void RegisterControllerServices(Container container, object context = null)
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
            container.RegisterInstance(serviceManager.GetService<IContainerService>().Container.GetInstance<ILobbyStateManager>());
            container.RegisterInstance(serviceManager.GetService<IContainerService>().Container.GetInstance<IGamePlayState>());
            container.RegisterInstance(serviceManager.GetService<IContainerService>().Container.GetInstance<IGameProvider>());
            container.RegisterInstance(serviceManager.GetService<IContainerService>().Container.GetInstance<IBank>());
            container.RegisterInstance(serviceManager.GetService<IContainerService>().Container.GetInstance<IPathMapper>());
            container.RegisterInstance(serviceManager.GetService<IContainerService>().Container.GetInstance<IGameService>());

            container.Register<RobotLogger>(Lifestyle.Singleton);
            container.Register<Automation>(Lifestyle.Singleton);
            container.Register<StateChecker>(Lifestyle.Singleton);

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
        }
    }
}
