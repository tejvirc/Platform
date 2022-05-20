namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using System;
    using Application.Contracts;
    using Contracts;

    /// <summary>
    ///     Handles the <see cref="SystemDisabledByOperatorEvent" /> event.
    /// </summary>
    public class SystemDisabledByOperatorEventConsumer : Consumes<SystemDisabledByOperatorEvent>
    {
        private readonly ICurrentMachineModeStateManager _currentMachineModeStateManager;

        public SystemDisabledByOperatorEventConsumer(ICurrentMachineModeStateManager currentMachineModeStateManager)
        {
            _currentMachineModeStateManager = currentMachineModeStateManager ?? throw new ArgumentNullException(nameof(currentMachineModeStateManager));
        }
        public override void Consume(SystemDisabledByOperatorEvent theEvent)
        {
            _currentMachineModeStateManager.HandleEvent(theEvent);
        }
    }
}