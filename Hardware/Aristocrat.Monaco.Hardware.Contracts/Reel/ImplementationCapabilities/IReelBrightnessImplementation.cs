namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ImplementationCapabilities
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    ///     The reel controller brightness capability of an implementation
    /// </summary>
    public interface IReelBrightnessImplementation : IReelImplementationCapability
    {
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
