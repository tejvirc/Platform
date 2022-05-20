namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>
    /// Provides access to ConfigWizardConfiguration in an injectable, unit-testable form
    /// </summary>
    public interface IConfigurationUtilitiesProvider 
    {
        /// <summary>
        /// Returns ConfigWizardConfiguration or the provided default value
        /// </summary>
        /// <param name="defaultOnError">The configuration to return if it couldn't be loaded normally</param>
        /// <returns>Either loaded configuration, or defaultOnError value</returns>
        ConfigWizardConfiguration GetConfigWizardConfiguration(Func<ConfigWizardConfiguration> defaultOnError);
    }
}