namespace Aristocrat.Monaco.Hardware.Contracts.ButtonDeck
{
    using System;
    using SharedDevice;

    /// <summary>Definition of the IButton interface.</summary>
    [CLSCompliant(false)]
    public interface IButtonDeckDisplay : IFirmwareCrcBridge
    {
        /// <summary>
        ///     Gets a value indicating the number of physical button deck displays connected.
        /// </summary>
        int DisplayCount { get; }

        /// <summary>
        ///     Gets a value indicating whether the hardware is being simulated.
        /// </summary>
        bool IsSimulated { get; }

        /// <summary>
        ///     Gets the seed used for the CRC calculation
        /// </summary>
        int Seed { get; }

        /// <summary>
        ///     Gets the firmware Id of the display at the given index.
        /// </summary>
        /// <param name="displayIndex">
        ///     USB display index; for example, gen8 button deck display has two
        ///     USB displays (one for bet buttons and one for bash button).
        /// </param>
        /// <returns>The firmware Id for the given display index.</returns>
        string GetFirmwareId(int displayIndex);

        /// <summary>
        ///     Gets the rendered frame ID of the specified display.  In particular, this is used
        ///     for clients of this interface as a "dirty" optimization to know when the frame data
        ///     has changed.
        /// </summary>
        /// <param name="displayIndex">
        ///     USB display index; for example, gen8 button deck display has two
        ///     USB displays (one for bet buttons and one for bash button).
        /// </param>
        /// <returns>The frame Id.</returns>
        uint GetRenderedFrameId(int displayIndex);

        /// <summary>
        ///     Gets the rendered frame ID of the specified display.
        /// </summary>
        /// <param name="displayIndex">
        ///     USB display index; for example, gen8 button deck display has two
        ///     USB displays (one for bet buttons and one for bash button).
        /// </param>
        /// <returns>
        ///     The image data of the rendered frame or null.  For performance, this data
        ///     is not available when there is a physical button deck display, and the return value is null.
        /// </returns>
        byte[] GetRenderedFrame(int displayIndex);

        /// <summary>
        ///     Indicates to draws the image data to the button deck display from shared memory.
        /// </summary>
        void DrawFromSharedMemory();

        /// <summary>
        ///     Draws the image data to the button deck display.
        /// </summary>
        /// <param name="displayIndex">
        ///     USB display index; for example, gen8 button deck display has two
        ///     USB displays (one for bet buttons and one for bash button).
        /// </param>
        /// <param name="imageData">The image data we want to display on the button deck.</param>
        void Draw(int displayIndex, byte[] imageData);

        /// <summary>
        ///     Draws the image data to the button deck display.
        /// </summary>
        /// <param name="displayIndex">
        ///     USB display index; for example, gen8 button deck display has two
        ///     USB displays (one for bet buttons and one for bash button).
        /// </param>
        /// <param name="imageData">The image data we want to display on the button deck.</param>
        /// <param name="imageLength">The number of bytes in the image.</param>
        void Draw(int displayIndex, IntPtr imageData, int imageLength);
    }
}