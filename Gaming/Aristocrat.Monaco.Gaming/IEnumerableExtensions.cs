namespace Aristocrat.Monaco.Gaming
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class IEnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}