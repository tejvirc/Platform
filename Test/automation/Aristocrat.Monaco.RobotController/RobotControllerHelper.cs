namespace Aristocrat.Monaco.RobotController
{
    using SimpleInjector;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class RobotControllerHelper
    {
        internal static Configuration LoadConfiguration(string configPath)
        {
            return Configuration.Load(configPath);
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
                    container.GetInstance<BalanceOperations>(),
                    container.GetInstance<ServiceRequestOperations>(),
                    container.GetInstance<GameOperations>()
                },

                [nameof(ModeType.Super)] = new HashSet<IRobotOperations>
                {
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
