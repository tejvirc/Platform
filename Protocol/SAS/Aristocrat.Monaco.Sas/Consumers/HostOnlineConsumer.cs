namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Contracts.Events;
    using Gaming.Contracts.Progressives;
    using Progressive;

    /// <summary>
    ///     Handles the HostOnlineEvent consumer.
    ///     Notifies the <inheritdoc cref="IProtocolLinkedProgressiveAdapter"/> when a progressive host is online.
    /// </summary>
    public class HostOnlineConsumer : Consumes<HostOnlineEvent>
    {
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        /// <inheritdoc />
        public HostOnlineConsumer(IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ??
                                         throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
        }

        /// <inheritdoc />
        public override void Consume(HostOnlineEvent theEvent)
        {
            if (!theEvent.IsProgressiveHost)
            {
                return;
            }

            _protocolLinkedProgressiveAdapter.ReportLinkUp(ProgressiveConstants.ProtocolName);
        }
    }
}