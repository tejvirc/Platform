namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     
    /// </summary>
    public class RequestServiceResponse : Response
    {
        /// <summary>
        ///     Gets or sets the information needed to initiate a connection to the service.
        /// </summary>
        /// <remarks>
        ///     This will be an IP address and port, for example, "192.168.1.1:24055".
        /// </remarks>
        public string ConnectionString { get; set; }
    }
}
