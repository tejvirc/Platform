namespace Aristocrat.Monaco.G2S.Data.Model
{
    using Common.Storage;

    /// <summary>
    ///     Base class that represents serialized supported event data.
    /// </summary>
    public class SupportedEvent : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the device id.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the device class.
        /// </summary>
        public string DeviceClass { get; set; }

        /// <summary>
        ///     Gets or sets the event code.
        /// </summary>
        public string EventCode { get; set; }
    }
}