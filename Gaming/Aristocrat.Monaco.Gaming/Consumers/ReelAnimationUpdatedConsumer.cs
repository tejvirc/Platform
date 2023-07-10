namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using GdkRuntime.V1;
    using Google.Protobuf.WellKnownTypes;
    using Hardware.Contracts.Reel.Events;
    using Runtime;
    using AnimationState = Hardware.Contracts.Reel.AnimationState;
    using GDKAnimationState = GdkRuntime.V1.AnimationState;

    /// <summary>
    ///     Handles the <see cref="ReelAnimationUpdatedEvent" /> and sends an event to the runtime.
    /// </summary>
    public class ReelAnimationUpdatedConsumer : Consumes<ReelAnimationUpdatedEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelAnimationUpdatedConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public ReelAnimationUpdatedConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(ReelAnimationUpdatedEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            var updateNotification = new AnimationUpdatedNotification()
            {
                AnimationId = theEvent.AnimationName,
                AnimationData = Any.Pack(new ReelAnimationData { ReelIndex = theEvent.ReelIndex }),
                State = ToGdkAnimationState(theEvent.State)
            };

            _reelService.AnimationUpdated(updateNotification);
        }

        private GDKAnimationState ToGdkAnimationState(AnimationState preparedState)
        {
            return preparedState switch
            {
                AnimationState.Prepared => GDKAnimationState.AnimationsPrepared,
                AnimationState.Stopped => GDKAnimationState.AnimationStopped,
                AnimationState.Started => GDKAnimationState.AnimationPlaying,
                _ => throw new ArgumentOutOfRangeException(nameof(preparedState))
            };
        }
    }
}