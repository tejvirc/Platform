namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    ///     The public interface for reel controller brightness capabilities
    /// </summary>
    public interface IReelBrightnessCapabilities : IReelControllerCapability
    {
        ///<summary>
        ///     Get or sets the default brightness of the reel lights
        /// </summary>
        int DefaultReelBrightness { get; set; }

        /// <summary>
        ///     Sets the brightness for the reel lights
        /// </summary>
        /// <param name="brightness">The brightness to set for the lights</param>
        /// <returns>Whether or not the lamp brightness was updated</returns>
        Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness);

        /// <summary>
        ///     Sets the brightness for the all reel lights
        /// </summary>
        /// <param name="brightness">The brightness to set for the lights</param>
        /// <returns>Whether or not the lamp brightness was updated</returns>
        Task<bool> SetBrightness(int brightness);
    }
}
