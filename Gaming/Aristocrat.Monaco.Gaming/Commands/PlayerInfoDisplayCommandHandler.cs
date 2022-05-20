namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts.Events;
    using Kernel;

    /// <summary>
    ///     Command handler for the <see cref="PlayerInfoDisplayEnterRequest" />, <see cref="PlayerInfoDisplayExitRequest" />  command.
    ///     Handle Runtime Requests for Player Information Display
    /// </summary>
    public sealed  class PlayerInfoDisplayCommandHandler :
        ICommandHandler<PlayerInfoDisplayEnterRequest>,
        ICommandHandler<PlayerInfoDisplayExitRequest>
    {
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerInfoDisplayCommandHandler" /> class.
        /// </summary>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        public PlayerInfoDisplayCommandHandler(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public void Handle(PlayerInfoDisplayEnterRequest command)
        {
            _eventBus.Publish(new PlayerInfoButtonPressedEvent());
        }

        /// <inheritdoc />
        public void Handle(PlayerInfoDisplayExitRequest command)
        {
            _eventBus.Publish(new PlayerInfoDisplayExitRequestEvent());
        }
    }
}