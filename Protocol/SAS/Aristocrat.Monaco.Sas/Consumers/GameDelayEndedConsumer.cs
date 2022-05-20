namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Contracts.Client;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="GameDelayEndedEvent"/> event. 
    /// </summary>
    public class GameDelayEndedConsumer : Consumes<GameDelayEndedEvent>
    {
        private readonly IMessageDisplay _messageDisplay;

        /// <summary>
        ///     Initializes a new instance of <see cref="GameDelayEndedEvent"/> class.
        /// </summary>
        /// <param name="messageDisplay"></param>
        public GameDelayEndedConsumer(IMessageDisplay messageDisplay)
        {
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
        }

        /// <inheritdoc />
        public override void Consume(GameDelayEndedEvent evt)
        {
            _messageDisplay.RemoveMessage(GameDelayMessage.DisplayMessage);
        }
    }
}
