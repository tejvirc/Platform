namespace Aristocrat.Monaco.Asp.Events
{
    using Kernel;

    /// <summary>
    ///     LinkStatusChangedEvent class used by EventBus to send out notification when link layer status is changed
    /// </summary>
    public class LinkStatusChangedEvent : BaseEvent
    {
        public LinkStatusChangedEvent(bool isLinkup) => this.IsLinkUp = isLinkup;

        /// <summary>
        ///     Gets if DataLinkLayer communication with external systems is up and running.
        /// </summary>
        public bool IsLinkUp { get; }
    }
}
