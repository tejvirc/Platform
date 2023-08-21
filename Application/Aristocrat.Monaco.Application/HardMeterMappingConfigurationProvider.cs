namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Kernel;

    /// <inheritdoc cref="IHardMeterMappingConfigurationProvider" />
    public class HardMeterMappingConfigurationProvider : IHardMeterMappingConfigurationProvider, IService
    {
        private const string HardMeterMappingConfigurationAddin = "/HardMeterMapping/Configuration";

        /// <inheritdoc />
        public HardMeterMappingConfiguration GetHardMeterMappingConfiguration(Func<HardMeterMappingConfiguration> defaultOnError)
        {
            return ConfigurationUtilities.GetConfiguration(HardMeterMappingConfigurationAddin, defaultOnError);
        }

        public string Name => nameof(HardMeterMappingConfigurationProvider);

        public ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IHardMeterMappingConfigurationProvider) };

        public void Initialize()
        {
        }
    }
}