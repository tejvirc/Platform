namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Common.Events;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="ProgressiveHostOnlineEvent" /> event.
    /// </summary>
    public class ProgressiveHostOnlineConsumer : Consumes<ProgressiveHostOnlineEvent>
    {
        private readonly ISystemDisableManager _disableManager;

        /// <summary>
        ///     Initializes a new instance of the ProgressiveHostOnlineConsumer class.
        /// </summary>
        public ProgressiveHostOnlineConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            ISystemDisableManager disableManager)
            : base(eventBus, consumerContext)
        {
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
        }

        /// <inheritdoc />
        public override void Consume(ProgressiveHostOnlineEvent @event)
        {
            _disableManager.Enable(BingoConstants.ProgresssiveHostOfflineKey);
        }
    }
}
