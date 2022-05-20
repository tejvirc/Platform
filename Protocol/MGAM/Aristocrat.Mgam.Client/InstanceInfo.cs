namespace Aristocrat.Mgam.Client
{
    using System;

    /// <summary>
    ///     Stores data about the device registration information with the VLT service.
    /// </summary>
    public class InstanceInfo
    {
        /// <summary>
        ///     Gets or sets the InstanceId returned in the RegisterInstanceResponse message.
        /// </summary>
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets or sets the Address returned in the RequestServiceResponse message.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the DeviceGuid returned in the RegisterInstanceResponse message.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the DeviceGuid returned in the RegisterInstanceResponse message.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Gets or sets the SiteId returned in the RegisterInstanceResponse message.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        ///     Gets or sets the Description returned in the RegisterInstanceResponse message.
        /// </summary>
        public string Description { get; set; }
    }
}
