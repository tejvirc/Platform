namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using GdkRuntime.V1;
    using Hardware.Contracts.Reel.Events;
    using Runtime;

    /// <summary>
    ///     Handles the <see cref="AllLightAnimationsClearedEvent" /> and sends an event to the runtime.
    /// </summary>
    public class AllLightAnimationsClearedConsumer : Consumes<AllLightAnimationsClearedEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AllLightAnimationsClearedConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public AllLightAnimationsClearedConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(AllLightAnimationsClearedEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            var updateNotification = new AnimationUpdatedNotification
            {
                AnimationId = string.Empty,
                AnimationData = null,
                State = AnimationState.AllAnimationsCleared
            };

            _reelService.NotifyAnimationUpdated(updateNotification);
        }
    }
}