namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Collections.Generic;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Events;
    using Runtime;

    /// <summary>
    ///     Handles the ReelAcceleratingEvent and sends an event to the runtime.
    /// </summary>
    public class ReelAcceleratingConsumer : Consumes<ReelAcceleratingEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelAcceleratingConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public ReelAcceleratingConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(ReelAcceleratingEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            var logicalState = theEvent.Direction == SpinDirection.Forward ?
                ReelLogicalState.SpinningForwardAccelerating :
                ReelLogicalState.SpinningBackwardsAccelerating;

            var reelState = new Dictionary<int, ReelLogicalState>
            {
                { theEvent.ReelId, logicalState }
            };

            _reelService.UpdateReelState(reelState);
        }
    }
}
