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
    ///     Handles the <see cref="LightShowAnimationUpdatedEvent" /> and sends an event to the runtime.
    /// </summary>
    public class LightShowAnimationUpdatedConsumer : Consumes<LightShowAnimationUpdatedEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LightShowAnimationUpdatedConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public LightShowAnimationUpdatedConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(LightShowAnimationUpdatedEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            var updateNotification = new AnimationUpdatedNotification()
            {
                AnimationId = theEvent.AnimationName,
                AnimationData = Any.Pack(new LightshowAnimationData { Tag = theEvent.Tag }),
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
                AnimationState.Removed => GDKAnimationState.AnimationRemoved,
                _ => throw new ArgumentOutOfRangeException(nameof(preparedState))
            };
        }
    }
}