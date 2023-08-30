namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Hardware.Contracts.Reel.Events;
    using Runtime;

    /// <summary>
    ///     Handles the <see cref="StepperRuleTriggeredEvent" /> and sends an event to the runtime.
    /// </summary>
    public class StepperRuleTriggeredConsumer : Consumes<StepperRuleTriggeredEvent>
    {
        private readonly IReelService _reelService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StepperRuleTriggeredConsumer" /> class.
        /// </summary>
        /// <param name="reelService">The reel service</param>
        public StepperRuleTriggeredConsumer(IReelService reelService)
        {
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
        }

        /// <inheritdoc />
        public override void Consume(StepperRuleTriggeredEvent theEvent)
        {
            if (!_reelService.Connected)
            {
                return;
            }

            _reelService.NotifyStepperRuleTriggered(theEvent.ReelId, theEvent.EventId);
        }
    }
}
