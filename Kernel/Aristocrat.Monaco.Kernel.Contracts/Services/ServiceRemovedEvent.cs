namespace Aristocrat.Monaco.Kernel
{
    using System;

    /// <summary>
    ///     This event is emitted whenever a service is removed from the IServiceManager implementation.
    /// </summary>
    [Serializable]
    public class ServiceRemovedEvent : ServiceEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceRemovedEvent" /> class.
        ///     A parameter less constructor is required for events that are sent from the
        ///     Key to Event converter.
        /// </summary>
        public ServiceRemovedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceRemovedEvent" /> class.
        /// </summary>
        /// <param name="serviceType">The type of service removed.</param>
        public ServiceRemovedEvent(Type serviceType)
            : base(serviceType)
        {
        }
    }
}