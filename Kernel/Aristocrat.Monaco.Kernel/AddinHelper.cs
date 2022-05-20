namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using Mono.Addins;

    /// <summary>
    ///     The AddinHelper class provides a service for other components to use that
    ///     allows them to retrieve addins as filtered by the currently selected
    ///     configuration property. If no selection has been made then the results
    ///     returned are unfiltered directly from the AddinManager.
    /// </summary>
    [CLSCompliant(false)]
    public class AddinHelper : IAddinHelper, IService
    {
        /// <summary>
        ///     Gets the selected nodes from the specified extension path
        /// </summary>
        /// <typeparam name="T">The type for the nodes</typeparam>
        /// <param name="extensionPath">The path to query for the nodes</param>
        /// <returns>The typed collection of the selected nodes</returns>
        public ICollection<T> GetSelectedNodes<T>(string extensionPath)
            where T : ExtensionNode
        {
            return MonoAddinsHelper.GetSelectedNodes<T>(extensionPath);
        }

        /// <summary>
        ///     Gets the name of the service
        /// </summary>
        public string Name => "AddinHelper";

        /// <summary>
        ///     Gets the service types supported by this service
        /// </summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(IAddinHelper) };

        /// <summary>
        ///     Initializes the service
        /// </summary>
        public void Initialize()
        {
        }
    }
}