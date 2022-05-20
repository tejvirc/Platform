namespace Aristocrat.G2S.Client
{
    using System.Collections.Generic;
    using Devices.v21;
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to persist event handler data.
    /// </summary>
    public interface IEventPersistenceManager
    {
        /// <summary>
        ///     Gets collection of the supported events.
        /// </summary>
        IReadOnlyCollection<object> SupportedEvents { get; }

        /// <summary>
        ///     Gets a all of the registered events for a device Id.
        /// </summary>
        /// <param name="hostId">Host Id.</param>
        /// <returns>Event host subscription list</returns>
        IEnumerable<eventHostSubscription> GetRegisteredEvents(int hostId);

        /// <summary>
        ///     Add host event subscriptions.
        /// </summary>
        /// <param name="subscriptions">Event host subscriptions</param>
        /// <param name="hostId">Host Id.</param>
        void RegisteredEvents(IEnumerable<eventHostSubscription> subscriptions, int hostId);

        /// <summary>
        ///     Deleted event host subscription.
        /// </summary>
        /// <param name="subscriptions">Event host subscriptions to delete</param>
        /// <param name="hostId">Host Id.</param>
        void RemoveRegisteredEventSubscriptions(IEnumerable<eventHostSubscription> subscriptions, int hostId);

        /// <summary>
        ///     Gets the set of all event subscriptions
        /// </summary>
        /// <param name="hostId">Host Id.</param>
        /// <returns>List of event subscriptions</returns>
        IEnumerable<object> GetAllEventSubscriptions(int hostId);

        /// <summary>
        ///     Gets the set of all event host subscriptions
        /// </summary>
        /// <param name="hostId">Host Id.</param>
        /// <returns>List of event host subscriptions</returns>
        IEnumerable<eventHostSubscription> GetAllRegisteredEventSub(int hostId);

        /// <summary>
        ///     Gets the set of all forced event subscriptions.
        /// </summary>
        /// <param name="hostId">Host Id.</param>
        /// <returns>List of the forced event subscriptions.</returns>
        IEnumerable<forcedSubscription> GetAllForcedEventSub(int hostId);

        /// <summary>
        ///     Gets forced event subscription.
        /// </summary>
        /// <param name="eventCode">Event Code</param>
        /// <param name="hostId">Host Id.</param>
        /// <param name="deviceId">Device Id.</param>
        /// <returns>Forced event subscription</returns>
        forcedSubscription GetForcedEvent(string eventCode, int hostId, int deviceId);

        /// <summary>
        ///     Adds forced event subscription.
        /// </summary>
        /// <param name="eventCode">Event Code</param>
        /// <param name="subscription">Forced event subscription data</param>
        /// <param name="hostId">Host Id.</param>
        /// <param name="deviceId">Device Id.</param>
        void AddForcedEvent(string eventCode, forcedSubscription subscription, int hostId, int deviceId);

        /// <summary>
        ///     Gets the current event Id
        /// </summary>
        /// <returns>Event Id</returns>
        long GetEventId();

        /// <summary>
        ///     Adds supported event.
        /// </summary>
        /// <param name="deviceClass">Device class name</param>
        /// <param name="deviceId">Device Id</param>
        /// <param name="eventCode">Event Code</param>
        void AddSupportedEvents(string deviceClass, int deviceId, string eventCode);

        /// <summary>
        ///     Removes supported event.
        /// </summary>
        /// <param name="deviceId">Device Id</param>
        /// <param name="eventCode">Event Code</param>
        void RemoveSupportedEvents(int deviceId, string eventCode);

        /// <summary>
        ///     Add event handler log.
        /// </summary>
        /// <param name="log">Event handler log</param>
        /// <param name="hostId">Device Id.</param>
        /// <param name="maxEntries">The maximum number of entities in the log</param>
        /// <returns>The current log count</returns>
        void AddEventLog(eventReport log, int hostId, int maxEntries);

        /// <summary>
        ///     Add event handler log.
        /// </summary>
        /// <param name="log">Event handler log</param>
        /// <param name="hostId">Device Id.</param>
        /// <param name="hostAck">Host acknowledged.</param>
        /// <returns>The current log count</returns>
        void UpdateEventLog(eventReport log, int hostId, bool hostAck);

        /// <summary>
        ///     Gets any unacknowledged event reports
        /// </summary>
        /// <param name="hostId">Device Id.</param>
        /// <returns>A list of unacknowledged events</returns>
        IReadOnlyCollection<eventReport> GetUnsentEvents(int hostId);

        /// <summary>
        ///     Adds default events
        /// </summary>
        /// <param name="deviceId">The device identifier</param>
        void AddDefaultEvents(int deviceId);

        /// <summary>
        ///     Gets the event report configurations for each event.
        /// </summary>
        /// <param name="deviceId">Device Id.</param>
        /// <param name="eventConfig">Event configs.</param>
        void SetEventReportConfigs(int deviceId, Dictionary<string, HashSet<EventReportConfig>> eventConfig);
    }
}