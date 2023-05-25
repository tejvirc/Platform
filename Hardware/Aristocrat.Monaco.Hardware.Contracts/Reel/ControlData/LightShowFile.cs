namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    /// <summary>
    ///     The data associated with a reel controller light show file
    /// </summary>
    public class LightShowFile
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte LoopCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte ReelIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public short Step { get; set; }
    }
}
