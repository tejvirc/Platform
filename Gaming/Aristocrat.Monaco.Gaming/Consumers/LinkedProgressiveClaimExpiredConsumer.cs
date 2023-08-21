namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;

    public class LinkedProgressiveClaimExpiredConsumer : Consumes<LinkedProgressiveClaimExpiredEvent>
    {
        private readonly IProgressiveErrorProvider _progressiveErrorProvider;

        public LinkedProgressiveClaimExpiredConsumer(IProgressiveErrorProvider progressiveErrorProvider)
        {
            _progressiveErrorProvider = progressiveErrorProvider ??
                                        throw new ArgumentNullException(nameof(progressiveErrorProvider));
        }

        public override void Consume(LinkedProgressiveClaimExpiredEvent theEvent)
        {
            _progressiveErrorProvider.ReportProgressiveClaimTimeoutError(theEvent.LinkedProgressiveLevels);
        }
    }
}