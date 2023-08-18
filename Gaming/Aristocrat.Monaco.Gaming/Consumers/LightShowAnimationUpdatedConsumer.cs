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
    ///     Handles the <see cref="LightAnimationUpdatedEvent" /> and sends an event to the runtime.
    /// </summary>
    public class LightShowAnimationUpdatedConsumer : Consumes<LightAnimationUpdatedEvent>
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
        public override void Consume(LightAnimationUpdatedEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            var updateNotification = new AnimationUpdatedNotification
            {
                AnimationId = theEvent.AnimationName,
                AnimationData = Any.Pack(new LightshowAnimationData { Tag = theEvent.Tag }),
                State = ToGdkAnimationState(theEvent.State)
            };

            _reelService.NotifyAnimationUpdated(updateNotification);
        }

        private static GDKAnimationState ToGdkAnimationState(AnimationState preparedState)
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