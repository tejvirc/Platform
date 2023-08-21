namespace Aristocrat.Monaco.Hardware.EdgeLight.Contracts
{
    using System;
    using Strips;

    /// <summary>
    ///     Interface for the Strip class that is used in the Edge light Manager
    /// </summary>
    public interface IStrip
    {
        /// <summary>
        ///     Get's the Strip ID (Fully qualified one with the BoardID.
        /// </summary>
        int StripId { get; }

        /// <summary>
        ///     Gets or sets the brightness in [0-100] range.
        /// </summary>
        int Brightness { get; set; }

        /// <summary>
        ///     Max Led's that can be supported in the strip
        /// </summary>
        int LedCount { get; }

        /// <summary>
        ///     The data in ARGB format for the Led
        /// </summary>

        LedColorBuffer ColorBuffer { get; }

        /// <summary>
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="sourceColorIndex"></param>
        /// <param name="ledCount"></param>
        /// <param name="destinationLedIndex"></param>
        void SetColors(
            LedColorBuffer segment,
            int sourceColorIndex,
            int ledCount,
            int destinationLedIndex);

        /// <summary>
        ///     Raised when strip brightness is changed.
        /// </summary>
        event EventHandler<EventArgs> BrightnessChanged;
    }
}