namespace Aristocrat.Monaco.Gaming.Commands
{
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData;

    /// <summary>
    ///     The <see cref="PrepareLightShows"/> class
    /// </summary>
    public class PrepareLightShows
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrepareLightShows" /> class.
        /// </summary>
        /// <param name="lightShowData">The light show data</param>
        public PrepareLightShows(params LightShowData[] lightShowData)
        {
            LightShowData = lightShowData;
        }

        /// <summary>
        ///     Gets the light show data
        /// </summary>
        public LightShowData[] LightShowData { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the light shows were prepared
        /// </summary>
        public bool Success { get; set; }
    }
}
