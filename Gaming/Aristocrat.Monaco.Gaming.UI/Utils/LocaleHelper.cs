namespace Aristocrat.Monaco.Gaming.UI.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class LocaleHelper
    {
        /// <summary>
        /// Check if the item is in the list, using passed in StringComparison when comparing.
        /// </summary>
        public static bool Includes(IEnumerable<string> list, string item, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase) => list.Any(l => string.Equals(l, item, comparison));

        /// <summary>
        /// Given an array of items, return the common elements present in all the items.
        /// </summary>
        public static IEnumerable<string> GetCommonLocales(IEnumerable<IEnumerable<string>> collections)
        {
            if (collections == null)
            {
                throw new ArgumentNullException(nameof(collections));
            }

            var lists = collections.ToArray();
            if (lists.Any())
            {
                var result = new HashSet<string>(lists.First(), StringComparer.OrdinalIgnoreCase);
                for (var i = 1; i < lists.Count(); i++)
                {
                    result.IntersectWith(lists.ElementAt(i));
                }

                return result;
            }

            return Enumerable.Empty<string>();
        }
    }
}
