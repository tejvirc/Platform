namespace Aristocrat.Monaco.Hhr.Client.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     A utility class for reflections
    /// </summary>
    public static class AssemblyUtilities
    {
        /// <summary>
        ///     Loads all types implementing T
        /// </summary>
        /// <param name="assemblies">Assemblies to look into.</param>
        /// <typeparam name="T">Base class/interface which classes should implement.</typeparam>
        /// <returns>List of classes implementing interface/class T</returns>
        public static IEnumerable<T> LoadAllTypesImplementing<T>(Assembly[] assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = new[] { typeof(T).Assembly };
            }

            foreach (var assembly in assemblies)
            {
                var timeoutBehaviors = assembly.GetExportedTypes()
                    .Where(p => typeof(T).IsAssignableFrom(p) && !p.IsInterface && p != typeof(T));
                foreach (var timeoutBehavior in timeoutBehaviors)
                {
                    yield return (T)Activator.CreateInstance(timeoutBehavior);
                }
            }
        }
    }
}