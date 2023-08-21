namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Contracts.Events;
    using Gaming.Contracts.Progressives;
    using Progressive;

    /// <summary>
    ///     Consumers the host offline event.
    ///     Notifies the <inheritdoc cref="IProtocolLinkedProgressiveAdapter"/> when a progressive host is offline
    /// </summary>
    public class HostOfflineConsumer : Consumes<HostOfflineEvent>
    {
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        /// <inheritdoc />
        public HostOfflineConsumer(IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                         throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
        }

        /// <inheritdoc />
        public override void Consume(HostOfflineEvent theEvent)
        {
            if (!theEvent.IsProgressiveHost)
            {
                return;
            }

            _protocolLinkedProgressiveAdapter.ReportLinkDown(ProgressiveConstants.ProtocolName);
        }
    }
}