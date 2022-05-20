namespace Aristocrat.G2S.Emdi.Extensions
{
    using System;
    using System.Linq;

    /// <summary>
    /// Extension methods for types
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Determines whether [is implementation of] [the specified interface type].
        /// </summary>
        /// <param name="this">The this.</param>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <returns>
        ///     <c>true</c> if [is implementation of] [the specified interface type]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     if @this or interfaceType is null
        /// </exception>
        public static bool IsImplementationOf(this Type @this, Type interfaceType)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            return @this.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }
    }
}