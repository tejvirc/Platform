namespace Aristocrat.Monaco.Sas.Contracts.Events
{
    using Kernel;
    using System.Globalization;

    /// <summary>
    ///     Definition of the HostOnlineEvent class
    /// </summary>
    public class HostOnlineEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostOnlineEvent" /> class.
        /// </summary>
        /// <param name="host">The host number</param>
        /// <param name="isProgressiveHost">If the host is a progressive host</param>
        public HostOnlineEvent(int host, bool isProgressiveHost)
        {
            Host = host;
            IsProgressiveHost = isProgressiveHost;
        }

        /// <summary>
        ///     Gets the host number of the event
        /// </summary>
        public int Host { get; }

        /// <summary>
        ///     Gets a value indicating if the host is a progressive host
        /// </summary>
        public bool IsProgressiveHost { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"SAS Host {Host} Online");
        }
    }
}