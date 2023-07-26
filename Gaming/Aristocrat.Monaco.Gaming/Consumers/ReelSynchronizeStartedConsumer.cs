namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Events;
    using Runtime;

    /// <summary>
    ///     Handles the <see cref="ReelSynchronizeStartedEvent" /> and sends an event to the runtime.
    /// </summary>
    public class ReelSynchronizeStartedConsumer : Consumes<ReelSynchronizeStartedEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelSynchronizeStartedConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public ReelSynchronizeStartedConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(ReelSynchronizeStartedEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            _reelService.NotifyReelSynchronizeStarted(theEvent.ReelId);
        }
    }
}
