namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Provides a mechanism to interact with the Configuration Utilities class that isn't static
    /// </summary>
    public interface IConfigurationUtility : IService
    {
        /// <summary>
        ///     Reads the configuation for the specified extension path
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="extensionPath">The extension path</param>
        /// <param name="defaultOnError">The default value to use in the event of an error</param>
        /// <returns>The parsed configuration or the default value in the event of an error</returns>
        T GetConfiguration<T>(string extensionPath, Func<T> defaultOnError)
            where T : class;
    }
}
