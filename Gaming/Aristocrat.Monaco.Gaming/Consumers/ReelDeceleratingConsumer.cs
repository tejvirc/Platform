namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Collections.Generic;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Events;
    using Runtime;

    /// <summary>
    ///     Handles the ReelDeceleratingEvent and sends an event to the runtime.
    /// </summary>
    public class ReelDeceleratingConsumer : Consumes<ReelDeceleratingEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelDeceleratingConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public ReelDeceleratingConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(ReelDeceleratingEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            var reelState = new Dictionary<int, ReelLogicalState>
            {
                { theEvent.ReelId, ReelLogicalState.Decelerating }
            };

            _reelService.UpdateReelState(reelState);
        }
    }
}
