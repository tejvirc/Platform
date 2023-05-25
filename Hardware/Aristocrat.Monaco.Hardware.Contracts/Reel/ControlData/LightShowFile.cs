namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    /// <summary>
    ///     The data associated with a reel controller light show file
    /// </summary>
    public class LightShowFile
    {
        /// <summary>
        /// The Animation Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The tag hash
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// Number of times to loop the animation
        /// </summary>
        public byte LoopCount { get; set; }

        /// <summary>
        /// The reel to apply the animation to
        /// </summary>
        public byte ReelIndex { get; set; }

        /// <summary>
        /// The step at which to play the animation.
        /// Use -1 if this does not apply
        /// </summary>
        public short Step { get; set; }
    }
}
