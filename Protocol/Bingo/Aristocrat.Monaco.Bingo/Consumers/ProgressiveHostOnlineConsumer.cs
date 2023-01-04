namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Application.Contracts;
    using Common;
    using Common.Events;
    using Gaming.Contracts.Progressives;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="ProgressiveHostOnlineEvent" /> event.
    /// </summary>
    public class ProgressiveHostOnlineConsumer : Consumes<ProgressiveHostOnlineEvent>
    {
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly ISystemDisableManager _disableManager;

        /// <summary>
        ///     Initializes a new instance of the ProgressiveHostOnlineConsumer class.
        /// </summary>
        public ProgressiveHostOnlineConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            ISystemDisableManager disableManager)
            : base(eventBus, consumerContext)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
        }

        /// <inheritdoc />
        public override void Consume(ProgressiveHostOnlineEvent @event)
        {
            _protocolLinkedProgressiveAdapter.ReportLinkUp(ProtocolNames.Bingo);
            _disableManager.Enable(BingoConstants.ProgresssiveHostOfflineKey);
        }
    }
}
