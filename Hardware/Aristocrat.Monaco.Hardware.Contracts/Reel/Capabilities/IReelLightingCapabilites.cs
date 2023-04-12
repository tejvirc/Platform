namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ControlData;

    /// <summary>
    ///     The public interface for reel controller lighting capabilities
    /// </summary>
    public interface IReelLightingCapabilities : IReelControllerCapability
    {
        /// <summary>
        ///     Requests a list of reel light identifiers for the reel controller.
        /// </summary>
        /// <returns>A list of reel light identifiers for the reel controller</returns>
        Task<IList<int>> GetReelLightIdentifiers();

        /// <summary>
        ///     Sets the requested lamp data
        /// </summary>
        /// <param name="lampData">The lamp data to set for the reels</param>
        /// <returns>Whether or not the lamps were updated</returns>
        Task<bool> SetLights(params ReelLampData[] lampData);
    }
}
