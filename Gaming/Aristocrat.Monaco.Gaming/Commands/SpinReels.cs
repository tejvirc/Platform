namespace Aristocrat.Monaco.Gaming.Commands
{
    using ReelSpinData = Hardware.Contracts.Reel.ControlData.ReelSpinData;

    /// <summary>
    ///     Spins the mechanical reels
    /// </summary>
    public class SpinReels
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SpinReels" /> class.
        /// </summary>
        /// <param name="spinData">The reel spin data</param>
        public SpinReels(params ReelSpinData[] spinData)
        {
            SpinData = spinData;
        }

        /// <summary>
        ///     Gets the reel spin data
        /// </summary>
        public ReelSpinData[] SpinData { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the reel can spin
        /// </summary>
        public bool Success { get; set; }
    }
}