namespace Aristocrat.Monaco.G2S.Data.Model
{
    using Common.Storage;

    /// <summary>
    ///     Event subscription type
    /// </summary>
    public enum EventSubscriptionType
    {
        /// <summary>
        ///     Forced event subscription
        /// </summary>
        Forced,

        /// <summary>
        ///     Host event subscription
        /// </summary>
        Host,

        /// <summary>
        ///     Host event subscription
        /// </summary>
        Permanent
    }

    /// <summary>
    ///     Base class that represents serialized event subscription data.
    /// </summary>
    public class EventSubscription : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the host id.
        /// </summary>
        public int HostId { get; set; }

        /// <summary>
        ///     Gets or sets the device id.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the event code.
        /// </summary>
        public string EventCode { get; set; }

        /// <summary>
        ///     Gets or sets subscription type.
        /// </summary>
        public EventSubscriptionType SubType { get; set; }

        /// <summary>
        ///     Gets or sets device class.
        /// </summary>
        public string DeviceClass { get; set; }

        /// <summary>
        ///     Gets or sets event persist flag.
        /// </summary>
        public bool EventPersist { get; set; }

        /// <summary>
        ///     Gets or sets sendClassMeters flag.
        /// </summary>
        public bool SendClassMeters { get; set; }
        
        /// <summary>
        ///     Gets or sets sendDeviceMeters flag.
        /// </summary>
        public bool SendDeviceMeters { get; set; }


        /// <summary>
        ///     Gets or sets sendDeviceStatus flag.
        /// </summary>
        public bool SendDeviceStatus { get; set; }


        /// <summary>
        ///     Gets or sets sendTransaction flag.
        /// </summary>
        public bool SendTransaction { get; set; }


        /// <summary>
        ///     Gets or sets sendUpdatableMeters flag.
        /// </summary>
        public bool SendUpdatableMeters { get; set; }
    }
}