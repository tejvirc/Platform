namespace Aristocrat.Monaco.Application.Contracts
{
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a mechanism to monitor events and control TowerLight device as defined in the configuration file.
    /// </summary>
    public interface ITowerLightManager : IService
    {
        /// <summary>
        ///     A list of available LightTiers from the tower light configuration
        /// </summary>
        IEnumerable<LightTier> ConfiguredLightTiers { get; }

        /// <summary>
        ///     Restarts the tower light conditions.
        /// </summary>
        void ReStart();

        /// <summary>
        ///     Gets a value indicating whether tower lights are disabled.
        /// </summary>
        bool TowerLightsDisabled { get; }
    }
}
