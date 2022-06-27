namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Commands;
    using Contracts;
    using Contracts.Events;
    using Runtime.Client;

    /// <summary>
    ///     Handles the GameRequestedPlatformHelpEvent
    /// </summary>
    public class HelpExitConsumer : Consumes<ExitHelpEvent>
    {
        private readonly ICommandHandlerFactory _handlerFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HelpExitConsumer" /> class.
        /// </summary>
        /// <param name="handlerFactory">An <see cref="ICommandHandlerFactory" /> instance.</param>
        public HelpExitConsumer(
            ICommandHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        /// <inheritdoc />
        public override void Consume(ExitHelpEvent theEvent)
        {
            _handlerFactory.Create<RuntimeRequest>()
                    .Handle(new RuntimeRequest(RuntimeRequestState.EndPlatformHelp));
        }
    }
}
