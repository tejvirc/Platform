namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal static class RobotControllerExtensions
    {
        internal static void BlockOtherOperations(this RobotController robotController, RobotStateAndOperations robotStateAndOperations)
        {
            robotController.InProgressRequests.TryAdd(robotStateAndOperations);
            Thread.Sleep(50);//this is needed since there are some on going thread executing operations
        }

        internal static void UnBlockOtherOperations(this RobotController robotController, RobotStateAndOperations robotStateAndOperations)
        {
            robotController.InProgressRequests.TryRemove(robotStateAndOperations);
        }

        internal static bool IsBlockedByOtherOperation(this RobotController robotController, IList<RobotStateAndOperations> excluded)
        {
            Func<RobotStateAndOperations, bool> predicate =
                (i) =>
                i != RobotStateAndOperations.SuperMode
                && i != RobotStateAndOperations.SuperMode
                && i != RobotStateAndOperations.SuperMode
                && !excluded.Contains(i);
            var isBlocked = robotController.InProgressRequests.Where(predicate).Any();
            return isBlocked;
        }
    }
}
