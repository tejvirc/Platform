namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     The <see cref="SetBrightness"/> class
    /// </summary>
    public class SetBrightness
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="SetBrightness"/> class.
        /// </summary>
        /// <param name="brightness">The brightness of the reels</param>
        public SetBrightness(uint brightness)
        {
            Brightness = brightness;
        }

        /// <summary>
        ///     the brightness
        /// </summary>
        public uint Brightness { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the stop was prepared
        /// </summary>
        public bool Success { get; set; }
    }
}
