namespace Aristocrat.Monaco.Asp.Client.Consumers
{
    using Contracts;
    using System;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="SystemDisableRemovedEvent" /> event.
    /// </summary>
    public class SystemDisableRemovedEventConsumer : Consumes<SystemDisableRemovedEvent>
    {
        private readonly IGameStatusProvider _gameStatusProvider;

        /// <summary>
        ///     Creates a SystemDisableAddedEventConsumer instance
        /// </summary>
        public SystemDisableRemovedEventConsumer(
            IGameStatusProvider gameStatusProvider)
        {
            _gameStatusProvider = gameStatusProvider ?? throw new ArgumentNullException(nameof(gameStatusProvider));
        }

        public override void Consume(SystemDisableRemovedEvent theEvent)
        {
            _gameStatusProvider.HandleEvent(theEvent);
        }
    }
}
