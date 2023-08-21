namespace Aristocrat.Monaco.Sas.Contracts
{
    using System.Collections.Generic;
    using System.Linq;
    using Kernel;

    /// <summary>
    ///     Extension methods for <see cref="IPropertiesManager"/>
    /// </summary>
    public static class PropertiesManagerExtensions
    {
        /// <summary>
        ///     Updates the property to the provided value if it is different
        /// </summary>
        /// <typeparam name="T">The type of object that will be set into the property</typeparam>
        /// <param name="this">The <see cref="IPropertiesManager"/></param>
        /// <param name="key">The property key that will updated</param>
        /// <param name="value">The value to set for the property</param>
        /// <returns>Whether or not the property was updated</returns>
        public static bool UpdateProperty<T>(this IPropertiesManager @this, string key, T value)
        {
            var currentValue = @this.GetValue(key, default(T));
            if (Equals(currentValue, value))
            {
                return false;
            }

            @this.SetProperty(key, value);
            return true;
        }

        /// <summary>
        ///     Updates the property to the provided value if it is different
        /// </summary>
        /// <typeparam name="T">The type of object that will be set into the property</typeparam>
        /// <param name="this">The <see cref="IPropertiesManager"/></param>
        /// <param name="key">The property key that will updated</param>
        /// <param name="value">The value to set for the property</param>
        /// <param name="comparer">The equality comparer to use</param>
        /// <returns>Whether or not the property was updated</returns>
        public static bool UpdateProperty<T>(this IPropertiesManager @this, string key, T value, IEqualityComparer<T> comparer)
        {
            var currentValue = @this.GetValue(key, default(T));
            if (comparer.Equals(currentValue, value))
            {
                return false;
            }

            @this.SetProperty(key, value);
            return true;
        }

        /// <summary>
        ///     Updates the property to the provided value if it is different
        /// </summary>
        /// <typeparam name="T">The type of object that will be set into the property</typeparam>
        /// <param name="this">The <see cref="IPropertiesManager"/></param>
        /// <param name="key">The property key that will updated</param>
        /// <param name="value">The value to set for the property</param>
        /// <param name="comparer">The equality comparer to use</param>
        /// <returns>Whether or not the property was updated</returns>
        public static bool UpdateProperty<T>(this IPropertiesManager @this, string key, IEnumerable<T> value, IEqualityComparer<T> comparer)
        {
            var currentValue = @this.GetValue(key, Enumerable.Empty<T>());
            if (currentValue.SequenceEqual(value, comparer))
            {
                return false;
            }

            @this.SetProperty(key, value);
            return true;
        }
    }
}