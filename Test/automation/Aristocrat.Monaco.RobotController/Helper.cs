namespace Aristocrat.Monaco.RobotController
{
    using SimpleInjector;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Helper
    {
        internal static Configuration LoadConfiguration(string configPath)
        {
            return Configuration.Load(configPath);
        }
        internal static void BlockOtherOperations(RobotController robotController, RobotStateAndOperations robotStateAndOperations)
        {
            robotController.InProgressRequests.TryAdd(robotStateAndOperations);
        }

        internal static void UnBlockOtherOperations(RobotController robotController, RobotStateAndOperations robotStateAndOperations)
        {
            robotController.InProgressRequests.TryRemove(robotStateAndOperations);
        }

        internal static bool IsBlockedByOtherOperation(RobotController robotController, IList<RobotStateAndOperations> excluded)
        {
            Func<RobotStateAndOperations, bool> predicate =
                (i) =>
                i != RobotStateAndOperations.SuperMode && i != RobotStateAndOperations.SuperMode && i != RobotStateAndOperations.SuperMode && !excluded.Contains(i);
            var isBlocked = robotController.InProgressRequests.Where(predicate).Any();
            return isBlocked;
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
