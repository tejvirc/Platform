namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts.Progressives;

    public class ProgressiveMinimumThresholdClearedConsumer : Consumes<ProgressiveMinimumThresholdClearedEvent>
    {
        private readonly IProgressiveErrorProvider _errorProvider;

        public ProgressiveMinimumThresholdClearedConsumer(IProgressiveErrorProvider errorProvider)
        {
            _errorProvider = errorProvider ?? throw new ArgumentNullException(nameof(errorProvider));
        }

        public override void Consume(ProgressiveMinimumThresholdClearedEvent theEvent)
        {
            _errorProvider.ClearMinimumThresholdError(theEvent.ProgressiveLevels);
        }
    }
}