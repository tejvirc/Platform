namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Progressives;

    public class LinkedProgressiveExpiredConsumer : Consumes<LinkedProgressiveExpiredEvent>
    {
        private readonly IProgressiveErrorProvider _progressiveErrorProvider;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly IPersistentStorageManager _persistentStorage;

        public LinkedProgressiveExpiredConsumer(
            IProgressiveErrorProvider progressiveErrorProvider,
            IProgressiveGameProvider progressiveGameProvider,
            IPersistentStorageManager persistentStorage)
        {
            _progressiveErrorProvider = progressiveErrorProvider ?? throw new ArgumentNullException(nameof(progressiveErrorProvider));
            _progressiveGameProvider = progressiveGameProvider ?? throw new ArgumentNullException(nameof(progressiveGameProvider));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
        }

        public override void Consume(LinkedProgressiveExpiredEvent theEvent)
        {
            _progressiveErrorProvider.ReportProgressiveUpdateTimeoutError(theEvent.NewlyExpiredLevels);

            using (var scope = _persistentStorage.ScopedTransaction())
            {
                _progressiveGameProvider.UpdateLinkedProgressiveLevels(theEvent.NewlyExpiredLevels);
                scope.Complete();
            }
        }
    }
}