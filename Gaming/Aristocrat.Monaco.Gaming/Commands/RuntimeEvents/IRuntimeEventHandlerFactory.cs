namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    using Runtime.Client;

    /// <summary>
    ///     Factory for runtime event handlers
    /// </summary>
    public interface IRuntimeEventHandlerFactory
    {
        /// <summary>
        ///     Creates the event handler for the type
        /// </summary>
        /// <param name="type">The game round event type</param>
        /// <returns>The event handler</returns>
        IRuntimeEventHandler Create(GameRoundEventState type);
    }
}
