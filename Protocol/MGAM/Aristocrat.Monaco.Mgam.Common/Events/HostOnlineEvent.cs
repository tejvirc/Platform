namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Published when site controller VLT services are available.
    /// </summary>
    public class HostOnlineEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostOnlineEvent"/> class.
        /// </summary>
        /// <param name="hostAddress"></param>
        public HostOnlineEvent(string hostAddress)
        {
            HostAddress = hostAddress;
        }

        /// <summary>
        ///     Gets the host address.
        /// </summary>
        public string HostAddress { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Host Online";
        }
    }
}
