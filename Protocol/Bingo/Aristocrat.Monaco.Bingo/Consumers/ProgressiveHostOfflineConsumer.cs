namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Application.Contracts;
    using Common;
    using Common.Events;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Handles the <see cref="ProgressiveHostOfflineEvent" /> event.
    /// </summary>
    public class ProgressiveHostOfflineConsumer : Consumes<ProgressiveHostOfflineEvent>
    {
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly ISystemDisableManager _disableManager;


        /// <summary>
        ///     Initializes a new instance of the ProgressiveHostOfflineConsumer class.
        /// </summary>
        public ProgressiveHostOfflineConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            ISystemDisableManager disableManager
        )
            : base(eventBus, consumerContext)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
        }

        /// <inheritdoc />
        public override void Consume(ProgressiveHostOfflineEvent @event)
        {
            _protocolLinkedProgressiveAdapter.ReportLinkDown(ProtocolNames.Bingo);
            _disableManager.Disable(
                BingoConstants.ProgresssiveHostOfflineKey,
                SystemDisablePriority.Normal,
                () => $"{Resources.ProgressiveHostDisconnected}");
        }
    }
}
