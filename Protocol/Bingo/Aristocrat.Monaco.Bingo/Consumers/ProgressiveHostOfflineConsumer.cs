namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Application.Contracts;
    using Common;
    using Common.Events;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Handles the <see cref="ProgressiveHostOfflineEvent" /> event.
    /// </summary>
    public class ProgressiveHostOfflineConsumer : Consumes<ProgressiveHostOfflineEvent>
    {
        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the ProgressiveHostOfflineConsumer class.
        /// </summary>
        public ProgressiveHostOfflineConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            ISystemDisableManager disableManager,
            IPropertiesManager propertiesManager)
            : base(eventBus, consumerContext)
        {
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public override void Consume(ProgressiveHostOfflineEvent @event)
        {
            // TODO make a new resource for "Progressive Host Disconnected"
            _disableManager.Disable(
                BingoConstants.ProgresssiveHostOfflineKey,
                SystemDisablePriority.Normal,
                () => $"{Resources.HostDisconnected}");
        }
    }
}
