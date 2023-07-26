namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Events;
    using Runtime;

    /// <summary>
    ///     Handles the <see cref="ReelSynchronizedEvent"/> and sends an event to the runtime.
    /// </summary>
    public class ReelSynchronizedConsumer : Consumes<ReelSynchronizedEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelSynchronizedConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public ReelSynchronizedConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(ReelSynchronizedEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            _reelService.NotifyReelSynchronized(theEvent.ReelId);
        }
    }
}
