namespace Aristocrat.Monaco.Mgam.Commands
{
    using System.Net;

    /// <summary>
    ///     Command for registering a VLT instance with a VLT service on the site controller.
    /// </summary>
    public class RegisterInstance
    {
        /// <summary>
        ///     Gets the address of the VLT service.
        /// </summary>
        public IPEndPoint EndPoint { get; set; }
    }
}
