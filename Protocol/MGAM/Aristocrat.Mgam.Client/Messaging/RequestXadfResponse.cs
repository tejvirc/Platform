namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     This message is received as a response to the RequestGUID message on the specified UDP port,
    ///     and contains the generated GUID.
    /// </summary>
    public class RequestXadfResponse : Response
    {
        /// <summary>
        ///     Gets or sets the Xadf.
        /// </summary>
        /// <remarks>
        ///     The name of the XADF file.  This file will be in the Manufacturer’s sub directory
        /// </remarks>
        public string Xadf { get; set; }

        /// <summary>
        ///     Gets or sets the FTP download server.
        /// </summary>
        /// <remarks>
        ///     The information needed to initiate a connection to the download server
        ///     in the form of username:password@address:port.
        ///     For example, “user:password@192.168.1.1:24055”
        ///     The username and password can contain any printable characters other than colon : and at @.
        /// </remarks>
        public string DownloadServer { get; set; }
    }
}