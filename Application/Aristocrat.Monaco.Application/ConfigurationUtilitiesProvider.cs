namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Kernel;

    /// <inheritdoc cref="IConfigurationUtilitiesProvider" />
    public class ConfigurationUtilitiesProvider : IConfigurationUtilitiesProvider, IService
    {
        private const string ConfigWizardExtensionPath = "/ConfigWizard/Configuration";

        /// <inheritdoc />
        public ConfigWizardConfiguration GetConfigWizardConfiguration(Func<ConfigWizardConfiguration> defaultOnError)
        {
            return ConfigurationUtilities.GetConfiguration(ConfigWizardExtensionPath, defaultOnError);
        }

        public string Name => nameof(ConfigurationUtilitiesProvider);

        public ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IConfigurationUtilitiesProvider) };

        public void Initialize(){}
    }
}