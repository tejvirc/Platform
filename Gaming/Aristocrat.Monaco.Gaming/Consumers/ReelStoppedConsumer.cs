namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Collections.Generic;
    using Hardware.Contracts.Reel;
    using Runtime;

    /// <summary>
    ///     Handles the ReelStoppedEvent and sends an event to the runtime.
    /// </summary>
    public class ReelStoppedConsumer : Consumes<ReelStoppedEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelStoppedConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public ReelStoppedConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(ReelStoppedEvent theEvent)
        {
            // Do not send reel stopped events to the runtime after homing
            if (!_reelService.Connected || theEvent.IsReelStoppedFromHoming)
            {
                return;
            }

            var reelState = new Dictionary<int, ReelLogicalState>
            {
                { theEvent.ReelId, ReelLogicalState.IdleAtStop }
            };

            _reelService.UpdateReelState(reelState);
        }
    }
}
