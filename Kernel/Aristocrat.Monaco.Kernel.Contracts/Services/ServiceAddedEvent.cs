namespace Aristocrat.Monaco.Kernel
{
    using System;

    /// <summary>
    ///     This event is emitted whenever a service is added to the IServiceManager implementation.
    /// </summary>
    [Serializable]
    public class ServiceAddedEvent : ServiceEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceAddedEvent" /> class.
        ///     A parameter less constructor is required for events that are sent from the
        ///     Key to Event converter.
        /// </summary>
        public ServiceAddedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceAddedEvent" /> class.
        /// </summary>
        /// <param name="serviceType">The type of service added.</param>
        public ServiceAddedEvent(Type serviceType)
            : base(serviceType)
        {
        }
    }
}