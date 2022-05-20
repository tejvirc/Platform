namespace Aristocrat.Monaco.Gaming.VideoLottery.Consumers
{
    using System;
    using Contracts.Lobby;
    using Runtime;
    using Runtime.Client;

    /// <summary>
    ///     Handles the TimeLimitDialogHiddenEvent.
    /// </summary>
    public class TimeLimitDialogHiddenConsumer : Consumes<TimeLimitDialogHiddenEvent>
    {
        private readonly IRuntime _runtime;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeLimitDialogHiddenConsumer" /> class.
        /// </summary>
        /// <param name="runtimeService">The IRuntimeService</param>
        public TimeLimitDialogHiddenConsumer(IRuntime runtimeService)
        {
            _runtime = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
        }

        /// <inheritdoc />
        public override void Consume(TimeLimitDialogHiddenEvent theEvent)
        {
            _runtime.UpdateFlag(RuntimeCondition.PlayTimeExpiredForceCashOut, false);
        }
    }
}