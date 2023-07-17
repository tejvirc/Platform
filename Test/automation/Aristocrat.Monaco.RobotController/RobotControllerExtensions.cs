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

            // This is needed since there are some on-going robot's threads executing operations and state managers that need more time to sync
            Thread.Sleep(_syncWaitTime);
        }

        internal static void UnBlockOtherOperations(this RobotController robotController, RobotStateAndOperations robotStateAndOperations)
        {
            robotController.InProgressRequests.TryRemove(robotStateAndOperations);
        }

        internal static bool IsBlockedByOtherOperation(this RobotController robotController, IList<RobotStateAndOperations> excluded)
        {
            var isBlocked = robotController.InProgressRequests.Any(Predicate);
            return isBlocked;

            bool Predicate(RobotStateAndOperations i)
            {
                return i != RobotStateAndOperations.SuperMode &&
                       i != RobotStateAndOperations.RegularMode &&
                       i != RobotStateAndOperations.UberMode &&
                       !excluded.Contains(i);
            }
        }

        internal  static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.RandomElementUsing<T>(new Random((int)DateTime.Now.Ticks));
        }

        internal static T RandomElementUsing<T>(this IEnumerable<T> enumerable, Random rand)
        {
            int index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }
    }
}
