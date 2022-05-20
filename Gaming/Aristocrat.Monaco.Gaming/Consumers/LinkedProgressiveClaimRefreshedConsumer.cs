namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;

    public class LinkedProgressiveClaimRefreshedConsumer : Consumes<LinkedProgressiveClaimRefreshedEvent>
    {
        private readonly IProgressiveErrorProvider _progressiveErrorProvider;

        public LinkedProgressiveClaimRefreshedConsumer(IProgressiveErrorProvider progressiveErrorProvider)
        {
            _progressiveErrorProvider = progressiveErrorProvider ??
                                        throw new ArgumentNullException(nameof(progressiveErrorProvider));
        }

        public override void Consume(LinkedProgressiveClaimRefreshedEvent theEvent)
        {
            _progressiveErrorProvider.ClearProgressiveClaimError(theEvent.LinkedProgressiveLevels);
        }
    }
}