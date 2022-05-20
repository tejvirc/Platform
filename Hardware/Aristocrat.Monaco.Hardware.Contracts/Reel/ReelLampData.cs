namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;

    /// <summary>
    ///     The data needed for setting the lamp color and state on the controller
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Data will be used once wired up to the game")]
    public class ReelLampData
    {
        /// <summary>
        ///     Creates the reel lamp data
        /// </summary>
        /// <param name="color">The color to set for the lamps</param>
        /// <param name="isLampOn">Whether or not to turn the lamps on or off</param>
        /// <param name="id">The identifier for the lamp to control</param>
        public ReelLampData(Color color, bool isLampOn, int id)
        {
            Color = color;
            IsLampOn = isLampOn;
            Id = id;
        }

        /// <summary>
        ///     Gets the color for the lamp
        /// </summary>
        public Color Color { get; }

        /// <summary>
        ///     Gets whether or not the lamp should be on
        /// </summary>
        public bool IsLampOn { get; }

        /// <summary>
        ///     Gets the identifier for the lamp
        /// </summary>
        public int Id { get; }
    }
}