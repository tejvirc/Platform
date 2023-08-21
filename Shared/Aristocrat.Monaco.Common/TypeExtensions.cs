namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Type extension methods
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Checks if a given type implements an interface
        /// </summary>
        /// <param name="this">The type to check</param>
        /// <param name="interfaceType">The interface it should implement</param>
        /// <returns>True if the type implements the interface</returns>
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

        /// <summary>
        ///     Gets the method based on the name and requested parameters
        /// </summary>
        /// <param name="this">The type to check</param>
        /// <param name="name">The method name</param>
        /// <param name="parameterTypes">The required parameters</param>
        /// <returns></returns>
        public static MethodInfo GetMethodEx(this Type @this, string name, Type[] parameterTypes)
        {
            var methods = @this.GetMethods();

            foreach (var method in methods.Where(m => m.Name == name))
            {
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
                {
                    return method;
                }
            }

            return null;
        }

        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x?.Assembly == y?.Assembly && x?.Namespace == y?.Namespace && x?.Name == y?.Name;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}