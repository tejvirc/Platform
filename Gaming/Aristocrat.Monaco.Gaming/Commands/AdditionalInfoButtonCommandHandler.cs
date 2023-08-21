namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts.Events;
    using Kernel;

    /// <summary>
    ///     Command handler for the <see cref="AdditionalInfoButton" /> command.
    /// </summary>
    public class AdditionalInfoButtonCommandHandler : ICommandHandler<AdditionalInfoButton>
    {
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AdditionalInfoButtonCommandHandler" /> class.
        /// </summary>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>

        public AdditionalInfoButtonCommandHandler(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public void Handle(AdditionalInfoButton command)
        {
            _eventBus.Publish(new AdditionalInfoButtonPressedEvent());
        }
    }
}