namespace Aristocrat.G2S.Client.Devices
{
    using System.Collections.Generic;
    using Protocol.v21;

    /// <summary>
    ///     Definition of the IEventHandlerDevice interface
    /// </summary>
    public interface IEventHandlerDevice : IDevice, IRestartStatus, ITransactionLogProvider
    {
        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the is a event overflows
        /// </summary>
        bool Overflow { get; set; }

        /// <summary>
        ///     Gets the disable behavior
        /// </summary>
        t_disableBehaviors DisableBehavior { get; }

        /// <summary>
        ///     Gets the queue behavior
        /// </summary>
        t_queueBehaviors QueueBehavior { get; }

        /// <summary>
        ///     Adds new event subscription
        /// </summary>
        /// <param name="subscriptions">A collection of event subscriptions</param>
        /// <returns>true if subscriptions were registered</returns>
        bool SetEventSubscriptions(IEnumerable<eventHostSubscription> subscriptions);

        /// <summary>
        ///     Gets the list of all event subscriptions.
        /// </summary>
        /// <returns>Event subscription list</returns>
        IEnumerable<object> GetAllEventSubscriptions();

        /// <summary>
        ///     Gets the list of all registered host event subscriptions.
        /// </summary>
        /// <returns>Event host subscription list</returns>
        IEnumerable<eventHostSubscription> GetAllRegisteredEventSub();

        /// <summary>
        ///     Gets the list of app forced event subscriptions.
        /// </summary>
        /// <returns>Forced event subscription list</returns>
        IEnumerable<forcedSubscription> GetAllForcedEventSub();

        /// <summary>
        ///     Deletes registered event host subscription.
        /// </summary>
        /// <param name="subscriptions">Event host subscriptions to be deleted.</param>
        void RemoveRegisteredEventSubscriptions(IEnumerable<eventHostSubscription> subscriptions);

        /// <summary>
        ///     Clears events subscriptions for a device.
        /// </summary>
        /// <param name="className">Device class name</param>
        /// <param name="deviceId">Device Id</param>
        void RemoveRegisteredHostEventSubscriptions(string className, int deviceId);

        /// <summary>
        ///     Updates states for the EventHandlerDevice instance
        /// </summary>
        void DeviceStateChanged();
    }
}