namespace Aristocrat.Monaco.Gaming.VideoLottery.Consumers
{
    using System;
    using Contracts.Lobby;
    using Runtime;
    using Runtime.Client;

    /// <summary>
    ///     Handles the TimeLimitDialogVisibleEvent.
    /// </summary>
    public class TimeLimitDialogVisibleConsumer : Consumes<TimeLimitDialogVisibleEvent>
    {
        private readonly IRuntime _runtime;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeLimitDialogVisibleConsumer" /> class.
        /// </summary>
        /// <param name="runtimeService">The IRuntimeService</param>
        public TimeLimitDialogVisibleConsumer(IRuntime runtimeService)
        {
            _runtime = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
        }

        /// <inheritdoc />
        public override void Consume(TimeLimitDialogVisibleEvent theEvent)
        {
            if (theEvent.IsLastPrompt)
            {
                _runtime.UpdateFlag(RuntimeCondition.PlayTimeExpiredForceCashOut, true);
            }
        }
    }
}