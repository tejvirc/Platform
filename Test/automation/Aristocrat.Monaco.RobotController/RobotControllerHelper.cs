namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using SimpleInjector;

    internal static class RobotControllerHelper
    {
        internal static Configuration LoadConfiguration(string configPath)
        {
            return Configuration.Load(configPath);
        }

        internal static HashSet<IRobotOperations> GetRobotOperationsForMode(Container container, string mode)
        {
            switch (mode)
            {
                case nameof(ModeType.Regular):
                    return new HashSet<IRobotOperations>()
                    {
                        container.GetInstance<CashoutOperations>(),
                        container.GetInstance<PlayerOperations>(),
                        container.GetInstance<TouchOperations>(),
                        container.GetInstance<BalanceOperations>(),
                        container.GetInstance<ServiceRequestOperations>(),
                        container.GetInstance<GameOperations>(),
                        container.GetInstance<GameHelpOperations>()
                    };

                case nameof(ModeType.Super):
                    return new HashSet<IRobotOperations>()
                    {
                        container.GetInstance<CashoutOperations>(),
                        container.GetInstance<PlayerOperations>(),
                        container.GetInstance<TouchOperations>(),
                        container.GetInstance<AuditMenuOperations>(),
                        container.GetInstance<BalanceOperations>(),
                        container.GetInstance<GameOperations>(),
                        container.GetInstance<ServiceRequestOperations>(),
                        container.GetInstance<LockUpOperations>(),
                        container.GetInstance<OperatingHoursOperations>(),
                        container.GetInstance<GameHelpOperations>()
                    };

                case nameof(ModeType.Uber):
                    return new HashSet<IRobotOperations>()
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
                        container.GetInstance<OperatingHoursOperations>(),
                        container.GetInstance<GameHelpOperations>()
                    };

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
    }
}
