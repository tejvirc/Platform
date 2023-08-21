namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using Contracts;
    using System;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="SystemDisableAddedEvent" /> event.
    /// </summary>
    public class SystemDisableAddedEventConsumer : Consumes<SystemDisableAddedEvent>
    {
        private readonly IGameStatusProvider _gameStatusProvider;

        /// <summary>
        ///     Creates a SystemDisableAddedEventConsumer instance
        /// </summary>
        public SystemDisableAddedEventConsumer(
            IGameStatusProvider gameStatusProvider)
        {
            _gameStatusProvider = gameStatusProvider ?? throw new ArgumentNullException(nameof(gameStatusProvider));
        }

        public override void Consume(SystemDisableAddedEvent theEvent)
        {
            _gameStatusProvider.HandleEvent(theEvent);
        }
    }
}
