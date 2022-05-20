namespace Aristocrat.G2S.Emdi.Host
{
    using Protocol.v21ext1b1;

    /// <summary>
    /// Holds information about the current request
    /// </summary>
    public class RequestContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestContext"/> class.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="config"></param>
        /// <param name="message"></param>
        public RequestContext(
            HostSession session, 
            HostConfiguration config, 
            mdMsg message)
        {
            Session = session;
            Config = config;
            Message = message;
        }

        /// <summary>
        /// Gets the current session
        /// </summary>
        public HostSession Session { get; }

        /// <summary>
        /// Gets the host configuration
        /// </summary>
        public HostConfiguration Config { get; }

        /// <summary>
        /// Gets the the request message sent from media display content
        /// </summary>
        public mdMsg Message { get; }
    }
}