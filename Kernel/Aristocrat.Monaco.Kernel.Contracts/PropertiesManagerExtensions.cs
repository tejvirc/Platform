namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Extension methods for the <see cref="IPropertiesManager" /> interface.
    /// </summary>
    public static class PropertiesManagerExtensions
    {
        /// <summary>
        ///     See <see cref="IPropertiesManager.GetProperty" /> for full documentation.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="this">The IPropertiesManager instance to act on.</param>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <param name="defaultValue">The default value for the property if it is not currently defined.</param>
        /// <returns>The value associated with the property name</returns>
        public static T GetValue<T>(this IPropertiesManager @this, string propertyName, T defaultValue)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return (T)@this.GetProperty(propertyName, defaultValue);
        }

        /// <summary>
        ///     See <see cref="IPropertiesManager.GetProperty" /> for full documentation.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="this">The IPropertiesManager instance to act on.</param>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>The value associated with the property name</returns>
        public static IEnumerable<T> GetValues<T>(this IPropertiesManager @this, string propertyName)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var list = @this.GetProperty(propertyName, null) as IEnumerable;

            return list?.Cast<T>() ?? Enumerable.Empty<T>();
        }
    }
}