namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Runtime;
    using Runtime.Client;

    public class AllowCombinedOutcomesConsumer : Consumes<AllowCombinedOutcomesEvent>
    {
        private readonly IRuntime _runtime;

        public AllowCombinedOutcomesConsumer(IRuntime runtime)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public override void Consume(AllowCombinedOutcomesEvent theEvent)
        {
            if (!_runtime.Connected)
            {
                return;
            }

            _runtime.UpdateFlag(RuntimeCondition.AllowCombinedOutcomes, theEvent.AllowCombineOutcomes);
        }
    }
}
