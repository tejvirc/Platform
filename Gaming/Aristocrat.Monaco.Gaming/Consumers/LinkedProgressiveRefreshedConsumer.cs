namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Progressives;

    public class LinkedProgressiveRefreshedConsumer : Consumes<LinkedProgressiveRefreshedEvent>
    {
        private readonly IProgressiveErrorProvider _progressiveErrorProvider;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly IPersistentStorageManager _persistentStorage;

        public LinkedProgressiveRefreshedConsumer(
            IProgressiveErrorProvider progressiveErrorProvider,
            IProgressiveGameProvider progressiveGameProvider,
            IPersistentStorageManager persistentStorage)
        {
            _progressiveErrorProvider = progressiveErrorProvider ??
                                        throw new ArgumentNullException(nameof(progressiveErrorProvider));
            _progressiveGameProvider = progressiveGameProvider ??
                                       throw new ArgumentNullException(nameof(progressiveGameProvider));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
        }

        public override void Consume(LinkedProgressiveRefreshedEvent theEvent)
        {
            _progressiveErrorProvider.ClearProgressiveUpdateError(theEvent.LinkedProgressiveLevels);

            using (var scope = _persistentStorage.ScopedTransaction())
            {
                _progressiveGameProvider.UpdateLinkedProgressiveLevels(theEvent.LinkedProgressiveLevels);
                scope.Complete();
            }
        }
    }
}