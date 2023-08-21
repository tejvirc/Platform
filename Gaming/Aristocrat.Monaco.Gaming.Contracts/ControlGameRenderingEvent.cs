namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     A <see cref="ControlGameRenderingEvent" /> should be posted when game should start/stop rendering.
    /// </summary>
    public class ControlGameRenderingEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="starting">True if starting game rendering, false if pausing.</param>
        public ControlGameRenderingEvent(bool starting)
        {
            Starting = starting;
        }

        /// <summary>
        ///     True if starting the game rendering, false if pausing.
        /// </summary>
        public bool Starting { get; }
    }
}