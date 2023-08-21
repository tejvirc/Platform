namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     An interface that need to be implemented by the protocol for handling Progressive events.
    /// </summary>
    public interface IProtocolProgressiveEventHandler
    {
        /// <summary>
        ///     A function that need to be implemented by the progressive event handlers in protocol.
        /// </summary>
        /// <param name="event"> the event data</param>
        void HandleProgressiveEvent<T>(T @event);
    }
}