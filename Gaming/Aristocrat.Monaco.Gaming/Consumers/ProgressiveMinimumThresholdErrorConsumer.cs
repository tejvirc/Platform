namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts.Progressives;

    public class ProgressiveMinimumThresholdErrorConsumer : Consumes<ProgressiveMinimumThresholdErrorEvent>
    {
        private readonly IProgressiveErrorProvider _errorProvider;

        public ProgressiveMinimumThresholdErrorConsumer(IProgressiveErrorProvider errorProvider)
        {
            _errorProvider = errorProvider ?? throw new ArgumentNullException(nameof(errorProvider));
        }

        public override void Consume(ProgressiveMinimumThresholdErrorEvent theEvent)
        {
            _errorProvider.ReportMinimumThresholdError(theEvent.ProgressiveLevels);
        }
    }
}