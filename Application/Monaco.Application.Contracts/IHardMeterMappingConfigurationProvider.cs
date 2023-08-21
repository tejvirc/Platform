namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>
    /// Provides access to HardMeterMappingConfiguration in an injectable, unit-testable form
    /// </summary>
    public interface IHardMeterMappingConfigurationProvider
    {
        /// <summary>
        /// Returns HardMeterMappingConfiguration or the provided default value
        /// </summary>
        /// <param name="defaultOnError">The configuration to return if it couldn't be loaded normally</param>
        /// <returns>Either loaded configuration, or defaultOnError value</returns>
        HardMeterMappingConfiguration GetHardMeterMappingConfiguration(Func<HardMeterMappingConfiguration> defaultOnError);
    }
}