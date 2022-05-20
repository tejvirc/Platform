namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Application.Contracts;
    using Progressives;

    public class LinkedProgressiveDisconnectedConsumer : Consumes<LinkedProgressiveDisconnectedEvent>
    {
        private readonly IProgressiveErrorProvider _progressiveErrorProvider;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly IPersistentStorageManager _storage;
        private readonly IMeterManager _meterManager;

        public LinkedProgressiveDisconnectedConsumer(
            IProgressiveErrorProvider progressiveErrorProvider,
            IProgressiveGameProvider progressiveGameProvider,
            IPersistentStorageManager storage,
            IMeterManager meterManager)
        {
            _progressiveErrorProvider = progressiveErrorProvider ??
                                        throw new ArgumentNullException(nameof(progressiveErrorProvider));
            _progressiveGameProvider = progressiveGameProvider ??
                                       throw new ArgumentNullException(nameof(progressiveGameProvider));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
        }

        public override void Consume(LinkedProgressiveDisconnectedEvent theEvent)
        {
            _progressiveErrorProvider.ReportProgressiveDisconnectedError(theEvent.LinkedProgressiveLevels);
            _meterManager.GetMeter(ApplicationMeters.ProgressiveDisconnectCount).Increment(1);

            using (var scope = _storage.ScopedTransaction())
            {
                // Update the progressive game provider for link down display
                _progressiveGameProvider.UpdateLinkedProgressiveLevels(theEvent.LinkedProgressiveLevels);

                scope.Complete();
            }
        }
    }
}