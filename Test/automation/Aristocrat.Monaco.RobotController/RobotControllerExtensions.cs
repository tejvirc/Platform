namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal static class RobotControllerExtensions
    {
        private static TimeSpan _syncWaitTime = TimeSpan.FromMilliseconds(200);
        internal static void BlockOtherOperations(this RobotController robotController, RobotStateAndOperations robotStateAndOperations)
        {
            robotController.InProgressRequests.TryAdd(robotStateAndOperations);
            // This is needed since there are some on going robot's threads executing operations and state managers need a more time to get synced
            Thread.Sleep(_syncWaitTime);
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
                && i != RobotStateAndOperations.RegularMode
                && i != RobotStateAndOperations.UberMode
                && !excluded.Contains(i);
            var isBlocked = robotController.InProgressRequests.Where(predicate).Any();
            return isBlocked;
        }
    }
}
