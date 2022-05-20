namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using System;
    using Application.Contracts;
    using Contracts;

    /// <summary>
    ///     Handles the <see cref="SystemEnabledByOperatorEvent" /> event.
    /// </summary>
    public class SystemEnabledByOperatorEventConsumer : Consumes<SystemEnabledByOperatorEvent>
    {
        private readonly ICurrentMachineModeStateManager _currentMachineModeStateManager;

        public SystemEnabledByOperatorEventConsumer(ICurrentMachineModeStateManager currentMachineModeStateManager)
        {
            _currentMachineModeStateManager = currentMachineModeStateManager ?? throw new ArgumentNullException(nameof(currentMachineModeStateManager));
        }
        public override void Consume(SystemEnabledByOperatorEvent theEvent)
        {
            _currentMachineModeStateManager.HandleEvent(theEvent);
        }
    }
}