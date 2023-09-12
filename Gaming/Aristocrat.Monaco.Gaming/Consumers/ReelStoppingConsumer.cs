namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Hardware.Contracts.Reel.Events;
    using Runtime;

    /// <summary>
    ///     Handles the <see cref="ReelStoppingEvent" /> and sends an event to the runtime.
    /// </summary>
    public class ReelStoppingConsumer : Consumes<ReelStoppingEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelStoppingConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public ReelStoppingConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(ReelStoppingEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            _reelService.NotifyReelStopping(theEvent.ReelId, theEvent.TimeToStop);
        }
    }
}
