namespace Aristocrat.Monaco.Gaming.Consumers
{
    using Kernel.Contracts.Events;

    public class InitializationCompletedConsumer : Consumes<InitializationCompletedEvent>
    {
        /// <summary>
        ///     Handles the event.
        /// </summary>
        /// <param name="event">The InitializationCompletedEvent to handle.</param>
        public override void Consume(InitializationCompletedEvent @event)
        {
            HandpayDisplayHelper.RecoverAlternativeCancelCreditTickerMessage();
        }
    }
}