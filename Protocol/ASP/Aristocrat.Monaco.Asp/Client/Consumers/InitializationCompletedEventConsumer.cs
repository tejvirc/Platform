namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using System;
    using Contracts;
    using Kernel.Contracts.Events;

    /// <summary>
    ///     Handles the <see cref="InitializationCompletedEvent" /> event.
    /// </summary>
    public class InitializationCompletedEventConsumer : Consumes<InitializationCompletedEvent>
    {
        private readonly ICurrentMachineModeStateManager _currentMachineModeStateManager;

        public InitializationCompletedEventConsumer(ICurrentMachineModeStateManager currentMachineModeStateManager)
        {
            _currentMachineModeStateManager = currentMachineModeStateManager ?? throw new ArgumentNullException(nameof(currentMachineModeStateManager));
        }
        public override void Consume(InitializationCompletedEvent theEvent)
        {
            _currentMachineModeStateManager.HandleEvent(theEvent);
        }
    }
}