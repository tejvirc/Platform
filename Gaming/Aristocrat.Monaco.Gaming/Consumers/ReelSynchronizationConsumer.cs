namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Hardware.Contracts.Reel.Events;
    using Runtime;

    /// <summary>
    ///     Handles the <see cref="ReelSynchronizationEvent" /> and sends an event to the runtime.
    /// </summary>
    public class ReelSynchronizationConsumer : Consumes<ReelSynchronizationEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelSynchronizationConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public ReelSynchronizationConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(ReelSynchronizationEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            _reelService.NotifyReelSynchronizationStatus(theEvent.ReelId, theEvent.Status);
        }
    }
}
