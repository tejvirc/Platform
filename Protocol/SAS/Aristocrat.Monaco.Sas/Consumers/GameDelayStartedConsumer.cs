namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Contracts.Client;
    using Gaming.Contracts;
    using Kernel.Contracts.MessageDisplay;

    /// <summary>
    ///     Handles the <see cref="GameDelayStartedEvent"/> event.
    /// </summary>
    public class GameDelayStartedConsumer : Consumes<GameDelayStartedEvent>
    {
        private readonly IMessageDisplay _messageDisplay;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDelayStartedEvent"/> class.
        /// </summary>
        /// <param name="messageDisplay">Displays the message to the user</param>
        public GameDelayStartedConsumer(IMessageDisplay messageDisplay)
        {
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
        }

        /// <inheritdoc />
        public override void Consume(GameDelayStartedEvent evt)
        {
            _messageDisplay.DisplayMessage(GameDelayMessage.DisplayMessage);
        }
    }
}
