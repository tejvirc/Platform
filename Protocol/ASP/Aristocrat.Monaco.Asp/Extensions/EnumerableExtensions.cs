namespace Aristocrat.Monaco.Asp.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Determines whether an element is part of the array of elements.
        /// </summary>
        public static bool IsAnyOf<T>(this T item, params T[] list) => list.Any(l => Comparer<T>.Default.Compare(l, item) == 0);

        /// <summary>
        ///     Iterate all the elements in the sequence, and apply the action to each one of the element.
        /// </summary>
        [DebuggerStepThrough]
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> a)
        {
            foreach (var l in list)
            {
                a(l);
            }
        }
    }
}