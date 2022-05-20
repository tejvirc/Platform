namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     A GameDrivenAttractEvent should be posted when game announces it has begun/ended attract mode
    /// </summary>
    public class GameDrivenAttractEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="running"></param>
        public GameDrivenAttractEvent(bool running)
        {
            Running = running;
        }

        /// <summary>
        ///     Whether game-driven attract is running
        /// </summary>
        public bool Running { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{GetType().Name} - {Running}";
        }
    }
}