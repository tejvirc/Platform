namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using System;
    using Contracts;
    using Hardware.Contracts.Persistence;

    /// <summary>
    ///     Handles the <see cref="PersistentStorageIntegrityCheckFailedEvent" /> event.
    /// </summary>
    public class PersistentStorageIntegrityCheckFailedEventConsumer : Consumes<PersistentStorageIntegrityCheckFailedEvent>
    {
        private readonly ICurrentMachineModeStateManager _currentMachineModeStateManager;

        public PersistentStorageIntegrityCheckFailedEventConsumer(ICurrentMachineModeStateManager currentMachineModeStateManager)
        {
            _currentMachineModeStateManager = currentMachineModeStateManager ?? throw new ArgumentNullException(nameof(currentMachineModeStateManager));
        }
        public override void Consume(PersistentStorageIntegrityCheckFailedEvent theEvent)
        {
            _currentMachineModeStateManager.HandleEvent(theEvent);
        }
    }
}