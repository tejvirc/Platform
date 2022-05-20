namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Published when site controller VLT services are unavailable.
    /// </summary>
    public class HostOfflineEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return "Host Offline";
        }
    }
}
