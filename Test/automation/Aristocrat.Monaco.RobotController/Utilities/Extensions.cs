namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public static class Extensions
    {
        public static T GetRandomElement<T>(this IList<T> items, Random rand)
        {
            return items[rand.Next(items.Count)];
        }

        public static T GetRandomElement<T>(this T[] items, Random rand)
        {
            return items[rand.Next(items.Length)];
        }

        public static T GetRandomElement<T>(this ISet<T> items, Random rand)
        {
            return items.ElementAt(rand.Next(items.Count));
        }

        public static void Halt(this Timer timer)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}