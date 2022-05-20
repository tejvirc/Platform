namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using System;
    using Contracts;
    using Hardware.Contracts.Persistence;

    /// <summary>
    ///     Handles the <see cref="StorageErrorEvent" /> event.
    /// </summary>
    public class StorageErrorEventConsumer : Consumes<StorageErrorEvent>
    {
        private readonly ICurrentMachineModeStateManager _currentMachineModeStateManager;

        public StorageErrorEventConsumer(ICurrentMachineModeStateManager currentMachineModeStateManager)
        {
            _currentMachineModeStateManager = currentMachineModeStateManager ?? throw new ArgumentNullException(nameof(currentMachineModeStateManager));
        }
        public override void Consume(StorageErrorEvent theEvent)
        {
            _currentMachineModeStateManager.HandleEvent(theEvent);
        }
    }
}