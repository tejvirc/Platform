namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Collections.Generic;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Events;
    using Runtime;

    /// <summary>
    ///     Handles the <see cref="ReelSpinningStatusUpdatedEvent"/> and sends an event to the runtime.
    /// </summary>
    public class ReelSpinningStatusUpdatedConsumer : Consumes<ReelSpinningStatusUpdatedEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelSpinningStatusUpdatedConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public ReelSpinningStatusUpdatedConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(ReelSpinningStatusUpdatedEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            ReelLogicalState logicalState;

            switch (theEvent.SpinVelocity)
            {
                case SpinVelocity.None:
                    return;
                case SpinVelocity.Constant:
                    logicalState = ReelLogicalState.SpinningConstant;
                    break;
                case SpinVelocity.Accelerating:
                    logicalState = ReelLogicalState.Accelerating;
                    break;
                case SpinVelocity.Decelerating:
                    logicalState = ReelLogicalState.Decelerating;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(theEvent.SpinVelocity));
            }

            var reelState = new Dictionary<int, ReelLogicalState>
            {
                { theEvent.ReelId, logicalState}
            };

            _reelService.UpdateReelState(reelState);
        }
    }
}
