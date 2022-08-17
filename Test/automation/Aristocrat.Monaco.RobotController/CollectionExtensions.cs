namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CollectionExtensions
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
    }
}
