namespace Aristocrat.Monaco.Hardware.Reel
{
    public struct ReelControllerOptions
    {
        /// <summary>Gets or sets the brightness of the reel lights</summary>
        public int ReelBrightness { get; set; }

        /// <summary>Gets or sets the offset for each reel</summary>
        public int[] ReelOffsets { get; set; }
    }
}
