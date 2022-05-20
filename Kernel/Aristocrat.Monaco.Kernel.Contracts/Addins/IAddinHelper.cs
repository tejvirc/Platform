namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using Mono.Addins;

    /// <summary>
    ///     Provides an interface that components can use to get extended addin loading functionality, instead of directly
    ///     using
    ///     the static MonoAddins​Helper
    /// </summary>
    [CLSCompliant(false)]
    public interface IAddinHelper
    {
        /// <summary>
        ///     Gets the selected nodes from the specified extension path
        /// </summary>
        /// <typeparam name="T">The type for the nodes</typeparam>
        /// <param name="extensionPath">The path to query for the nodes</param>
        /// <returns>The typed collection of the selected nodes</returns>
        ICollection<T> GetSelectedNodes<T>(string extensionPath)
            where T : ExtensionNode;
    }
}