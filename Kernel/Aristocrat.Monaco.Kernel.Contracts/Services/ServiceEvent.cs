namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Globalization;

    /// <summary>
    ///     This base class for events pertaining to services.
    /// </summary>
    [Serializable]
    public abstract class ServiceEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceEvent" /> class.
        ///     sets the 'ServiceType' property to null.
        ///     A parameter less constructor is required for events that are sent from the
        ///     Key to Event converter.
        /// </summary>
        protected ServiceEvent()
        {
            ServiceType = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceEvent" /> class.
        ///     sets the 'ServiceType' property to the one passed-in.
        /// </summary>
        /// <param name="serviceType">The type of service.</param>
        protected ServiceEvent(Type serviceType)
        {
            ServiceType = serviceType;
        }

        /// <summary>
        ///     Gets the type of service.
        /// </summary>
        public Type ServiceType { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [GUID={1}, ServiceType={2}]",
                GetType(),
                GloballyUniqueId,
                ServiceType);
        }
    }
}