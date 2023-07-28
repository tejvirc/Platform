namespace Aristocrat.Monaco.Gaming.Commands
{
    using Hardware.Contracts.Reel.ControlData;

    /// <summary>
    ///     The StopLightShowAnimations class
    /// </summary>
    public class StopLightShowAnimations
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StopLightShowAnimations" /> class.
        /// </summary>
        /// <param name="lightShowData">The light show data</param>
        public StopLightShowAnimations(params LightShowData[] lightShowData)
        {
            LightShowData = lightShowData;
        }

        /// <summary>
        ///     The light show data
        /// </summary>
        public LightShowData[] LightShowData { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not all animation with the identifier were stopped
        /// </summary>
        public bool Success { get; set; }
    }
}
