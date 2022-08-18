namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     The GameControlSizeChangedEvent event indicates that the size of the game control is changing.
    ///     This can happen when entering replay and the size needs to shrink.
    /// </summary>
    public class GameControlSizeChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameControlSizeChangedEvent" /> class.
        /// </summary>
        /// <param name="height">The new game control height.</param>
        public GameControlSizeChangedEvent(double height)
        {
            GameControlHeight = height;
        }

        /// <summary>
        ///     Gets the game control height.
        /// </summary>
        public double GameControlHeight { get; }
    }
}
