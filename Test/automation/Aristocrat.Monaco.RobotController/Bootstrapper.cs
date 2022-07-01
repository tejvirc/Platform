namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SimpleInjector;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Gaming;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System.Reflection;
    using System.IO;
    using Aristocrat.Monaco.Hardware.Contracts;

    internal class Bootstrapper
    {
        internal static Container InitializeContainer()
        {
            var serviceManager = ServiceManager.GetInstance();
            var container = new Container();
            var configPath = Path.Combine(ServiceManager.GetInstance().GetService<IPathMapper>().GetDirectory(HardwareConstants.DataPath).FullName,
                                            Constants.ConfigurationFileName);
            container.RegisterInstance(serviceManager.GetService<IEventBus>());
            container.RegisterInstance(serviceManager.GetService<IPropertiesManager>());
            container.RegisterInstance(serviceManager.GetService<IContainerService>().Container.GetInstance<ILobbyStateManager>()); //ILobbyStateManager is never added to the service manager directly. A prettier work around may exist
            container.RegisterInstance(serviceManager.GetService<IContainerService>().Container.GetInstance<IGamePlayState>());
            container.RegisterInstance(LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType));
            container.RegisterInstance(Configuration.Load(configPath));
            container.Register<RobotLogger>(Lifestyle.Singleton);
            container.Register<Automation>(Lifestyle.Singleton);
            container.Register<StateChecker>(Lifestyle.Singleton);
            container.Register<CashoutOperations>(Lifestyle.Singleton);
            container.Register<GameOperations>(Lifestyle.Singleton);
            container.Register<LobbyOperations>(Lifestyle.Singleton);
            container.Register<PlayerOperations>(Lifestyle.Singleton);
            container.Register<TouchOperations>(Lifestyle.Singleton);
            container.Register<LockUpOperations>(Lifestyle.Singleton);
            container.Register<OperatingHoursOperations>(Lifestyle.Singleton);
            container.Register<ServiceRequestOperations>(Lifestyle.Singleton);
            container.Register<BalanceOperations>(Lifestyle.Singleton);
            container.Register<RebootRequestOperations>(Lifestyle.Singleton);
            container.Register<AuditMenuOperations>(Lifestyle.Singleton);

            return container;
        }

        internal static Dictionary<string, HashSet<IRobotOperations>> InitializeModeDictionary(Container container)
        {
            var dict = new Dictionary<string, HashSet<IRobotOperations>>
            {
                [nameof(ModeType.Regular)] = new HashSet<IRobotOperations>
                {
                    container.GetInstance<CashoutOperations>(),
                    container.GetInstance<PlayerOperations>(),
                    container.GetInstance<TouchOperations>(),
                    container.GetInstance<AuditMenuOperations>(),
                    container.GetInstance<BalanceOperations>(),
                    container.GetInstance<GameOperations>(),
                    container.GetInstance<ServiceRequestOperations>(),
                    container.GetInstance<OperatingHoursOperations>()
                },

                [nameof(ModeType.Super)] = new HashSet<IRobotOperations>
                {
                    container.GetInstance<LobbyOperations>(),
                    container.GetInstance<CashoutOperations>(),
                    container.GetInstance<PlayerOperations>(),
                    container.GetInstance<TouchOperations>(),
                    container.GetInstance<AuditMenuOperations>(),
                    container.GetInstance<BalanceOperations>(),
                    container.GetInstance<GameOperations>(),
                    container.GetInstance<ServiceRequestOperations>(),
                    container.GetInstance<LockUpOperations>(),
                    container.GetInstance<OperatingHoursOperations>()
                },

                [nameof(ModeType.Uber)] = new HashSet<IRobotOperations>
                {
                    container.GetInstance<RebootRequestOperations>(),
                    container.GetInstance<LobbyOperations>(),
                    container.GetInstance<CashoutOperations>(),
                    container.GetInstance<PlayerOperations>(),
                    container.GetInstance<TouchOperations>(),
                    container.GetInstance<AuditMenuOperations>(),
                    container.GetInstance<BalanceOperations>(),
                    container.GetInstance<GameOperations>(),
                    container.GetInstance<ServiceRequestOperations>(),
                    container.GetInstance<LockUpOperations>(),
                    container.GetInstance<OperatingHoursOperations>()
                }
            };

            return dict;
        }
    }
}
