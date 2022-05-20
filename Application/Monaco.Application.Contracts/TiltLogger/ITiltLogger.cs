namespace Aristocrat.Monaco.Application.Contracts.TiltLogger
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     An interface by which all tilts and errors can be retrieved for display.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The component implementing this interface must save the log messages
    ///         so that other components can retrieve them for display or for other
    ///         purposes. If the amount of messages exceeds the maxium, it should
    ///         consider retaining the most recent ones. the component must also
    ///         be responsible for persisting all logged messages.
    ///     </para>
    ///     <para>
    ///         There is two events defined, one for tilt, another for error. A component
    ///         which wants to receive the most recently appended tilt or error should consider
    ///         hooking up to these events. The argument type being passed along with this event
    ///         is defined in <see cref="TiltLogAppendedEventArgs" />
    ///     </para>
    /// </remarks>
    public interface ITiltLogger
    {
        /// <summary>
        ///     Gets a value indicating whether or not to reload the history for the given event type.  Getting this
        ///     value will reset to false for the given event type so that views are reloaded only when that view has changed.
        /// </summary>
        /// <param name="type">The event type.</param>
        /// <param name="onLoaded">Indicates whether or not this was called from an OnLoaded method.</param>
        /// <returns>A value indicating whether or not to reload the history when a view model is loaded for the given event type.</returns>
        bool ReloadEventHistory(string type, bool onLoaded);

        /// <summary>
        ///     Gets the number of events by type parsed from the TiltLogger.config.xml file that will be subscribed to.
        /// </summary>
        /// <param name="type">The event type.</param>
        /// <returns>The number of events by type parsed from the TiltLogger.config.xml file that will be subscribed to.</returns>
        int GetEventsToSubscribe(string type);

        /// <summary>
        ///     Gets the number of events by type successfully subscribed to.
        /// </summary>
        /// <param name="type">The event type</param>
        /// <returns>the number of events by type successfully subscribed to.</returns>
        int GetEventsSubscribed(string type);

        /// <summary>
        ///     The event to notify that a new tilt is appended into the log.
        /// </summary>
        /// <remarks>
        ///     This event is usually hooked up by a component displaying tilts
        /// </remarks>
        event EventHandler<TiltLogAppendedEventArgs> TiltLogAppendedTilt;

        /// <summary>
        ///     Query the TiltLogger for recent events matching a given set of criteria.
        /// </summary>
        /// <param name="type">The event type</param>
        /// <returns>List of events formatted as EventDescription class.</returns>
        IEnumerable<EventDescription> GetEvents(string type);

        /// <summary>
        ///     Gets the maximum number of events for the given type.
        /// </summary>
        /// <param name="type">The event type</param>
        /// <returns>The maximum number of events.</returns>
        int GetMax(string type);

        /// <summary>
        ///     Gets the event types that are combined with the specified type to reach the max entries
        /// </summary>
        /// <param name="type">Event type</param>
        List<string> GetCombinedTypes(string type);
    }
}