namespace Aristocrat.Monaco.Gaming.Commands
{
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData;
    using System.Collections.Generic;

    /// <summary>
    ///     Prepare a number of light show animations
    /// </summary>
    public class PrepareLightShowAnimations
    {
        public PrepareLightShowAnimations(IReadOnlyCollection<LightShowData> lightShowData)
        {
            LightShowData = lightShowData;
        }

        /// <summary>
        /// The light show to be prepared
        /// </summary>
        public IReadOnlyCollection<LightShowData> LightShowData { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the light show was prepared
        /// </summary>
        public bool Success { get; set; }
    }
}