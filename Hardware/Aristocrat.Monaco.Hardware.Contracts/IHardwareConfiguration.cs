namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     Provides a mechanism to interact with the hardware configuration
    /// </summary>
    public interface IHardwareConfiguration : IService
    {
        /// <summary>
        ///     Gets the available configurations
        /// </summary>
        SupportedDevices SupportedDevices { get; }

        /// <summary>
        ///     Gets the current configuration
        /// </summary>
        /// <returns>The configuration</returns>
        IReadOnlyCollection<ConfigurationData> GetCurrent();

        /// <summary>
        ///     Applies the provided configuration
        /// </summary>
        /// <param name="configuration">The new configuration</param>
        /// <param name="inspect">Inspect the new devices.</param>
        void Apply(IReadOnlyCollection<ConfigurationData> configuration, bool inspect = true);
    }
}