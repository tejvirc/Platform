namespace Aristocrat.Monaco.G2S.Data.Model
{
    using System;
    using Common.Storage;

    /// <summary>
    ///     Base class that represents serialized event handler log data.
    /// </summary>
    public class EventHandlerLog : BaseEntity, ILogSequence
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
        ///     Gets or sets the transaction id.
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the event id.
        /// </summary>
        public long EventId { get; set; }

        /// <summary>
        ///  Gets or sets the device class.
        /// </summary>
        public string DeviceClass { get; set; }

        /// <summary>
        ///     Gets or sets the event code.
        /// </summary>
        public string EventCode { get; set; }

        /// <summary>
        ///     Gets or sets the event date time.
        /// </summary>
        public DateTime EventDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the event acknowledged by host.
        /// </summary>
        public bool EventAck { get; set; }

        /// <summary>
        /// Gets or sets transaction list.
        /// </summary>
        public string TransactionList { get; set; }

        /// <summary>
        /// Gets or sets device list.
        /// </summary>
        public string DeviceList { get; set; }

        /// <summary>
        /// Gets or sets Meter list.
        /// </summary>
        public string MeterList { get; set; }
    }
}