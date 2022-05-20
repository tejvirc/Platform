namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Provides a mechanism to discover and load all available runtime instances
    /// </summary>
    public interface IRuntimeProvider
    {
        /// <summary>
        ///     Gets the default runtime instance
        /// </summary>
        string DefaultInstance { get; }

        /// <summary>
        ///     Gets the appropriate runtime from a given pattern
        /// </summary>
        /// <param name="pattern">The regex pattern to match</param>
        /// <returns>The runtime instance or null if not found</returns>
        string FindTargetRuntime(Regex pattern);

        /// <summary>
        ///     Loads the runtime from an ISO
        /// </summary>
        void Load();

        /// <summary>
        ///     Unloads the runtime (un-mounts the ISO)
        /// </summary>
        void Unload();

        /// <summary>
        ///     Unloads the specified runtime (un-mounts the ISO)
        /// </summary>
        /// <param name="runtimeId">The runtime identifier (it's the package id)</param>
        void Unload(string runtimeId);
    }
}
