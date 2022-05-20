namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is sent in response to a <see cref="RegisterInstance" /> message.
    /// </summary>
    public class RegisterInstanceResponse : Response
    {
        /// <summary>
        ///     Gets or sets the description string assigned to the device. This parameter is
        ///     assigned by the site controller.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the current timestamp in the format “MM-DD-YYYY HH:MM:SS”
        ///     that shall be used to synchronize VLT clock with site controller
        ///     clock.
        /// </summary>
        public string TimeStamp { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier for the connection to the service that is used for all
        ///     subsequent communication with the service.
        /// </summary>
        public int InstanceId { get; set; }

        /// <summary>
        ///     Gets or sets the identifier that represents the site.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        ///     Gets or sets the unique identifier for the device being registered.
        /// </summary>
        public int DeviceId { get; set; }
    }
}