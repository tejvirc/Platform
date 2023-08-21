namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Kernel;

    /// <summary>
    ///     Implementation of <see cref="IConfigurationUtility" />
    /// </summary>
    public class ConfigurationUtility : IConfigurationUtility
    {
        /// <inheritdoc />
        public string Name => typeof(ConfigurationUtility).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IConfigurationUtility) };

        /// <inheritdoc />
        public T GetConfiguration<T>(string extensionPath, Func<T> defaultOnError)
            where T : class
        {
            return ConfigurationUtilities.GetConfiguration(extensionPath, defaultOnError);
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}
