namespace Aristocrat.Monaco.Sas.Contracts.Events
{
    using Kernel;
    using System.Globalization;

    /// <summary>
    ///     Definition of the HostOfflineEvent class
    /// </summary>
    public class HostOfflineEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostOfflineEvent" /> class.
        /// </summary>
        /// <param name="host">The host number</param>
        /// <param name="isProgressiveHost">If the host is a progressive host</param>
        public HostOfflineEvent(int host, bool isProgressiveHost)
        {
            Host = host;
            IsProgressiveHost = isProgressiveHost;
        }

        /// <summary>
        ///     Gets the host number of the event
        /// </summary>
        public int Host { get; }

        /// <summary>
        ///     Gets a value indicating if the host controls progressives
        /// </summary>
        public bool IsProgressiveHost { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"SAS Host {Host} Offline");
        }
    }
}