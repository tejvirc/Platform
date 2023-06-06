namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    using System;

    /// <summary>
    ///     The data associated with a reel controller light show file
    /// </summary>
    [CLSCompliant(false)]
    public class LightShowData
    {
        /// <summary>
        ///     The Animation Id
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        ///     The tag hash
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        ///     Number of times to loop the animation
        /// </summary>
        public sbyte LoopCount { get; set; }

        /// <summary>
        ///     The reel to apply the animation to
        /// </summary>
        public sbyte ReelIndex { get; set; }

        /// <summary>
        ///     The step at which to play the animation.
        ///     Use -1 if this does not apply
        /// </summary>
        public short Step { get; set; }
    }
}
