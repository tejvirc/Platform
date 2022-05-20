namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Progressives;

    public class LinkedProgressiveConnectedConsumer : Consumes<LinkedProgressiveConnectedEvent>
    {
        private readonly IProgressiveErrorProvider _progressiveErrorProvider;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly IPersistentStorageManager _storage;

        public LinkedProgressiveConnectedConsumer(
            IProgressiveErrorProvider progressiveErrorProvider,
            IProgressiveGameProvider progressiveGameProvider,
            IPersistentStorageManager storage)
        {
            _progressiveErrorProvider = progressiveErrorProvider ??
                                        throw new ArgumentNullException(nameof(progressiveErrorProvider));
            _progressiveGameProvider = progressiveGameProvider ??
                                       throw new ArgumentNullException(nameof(progressiveGameProvider));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public override void Consume(LinkedProgressiveConnectedEvent theEvent)
        {
            _progressiveErrorProvider.ClearProgressiveDisconnectedError(theEvent.LinkedProgressiveLevels);

            using (var scope = _storage.ScopedTransaction())
            {
                _progressiveGameProvider.UpdateLinkedProgressiveLevels(theEvent.LinkedProgressiveLevels);
                scope.Complete();
            }
        }
    }
}