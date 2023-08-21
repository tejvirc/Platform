namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Messaging;
    using Common;
    using Services.GameConfiguration;

    /// <summary>
    ///     Handles the <see cref="CustomCommand"/> message.
    /// </summary>
    public class CustomCommandHandler : MessageHandler<CustomCommand>
    {
        private readonly IGameConfigurator _gameConfigurator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomCommandHandler"/> class.
        /// </summary>
        /// <param name="gameConfigurator"><see cref="IGameConfigurator"/>.</param>
        public CustomCommandHandler(IGameConfigurator gameConfigurator)
        {
            _gameConfigurator = gameConfigurator;
        }

        /// <inheritdoc />
        public override async Task<IResponse> Handle(CustomCommand message)
        {
            switch ((CustomCommandCode)message.CommandId)
            {
                case CustomCommandCode.Configure:
                    await _gameConfigurator.Configure();
                    break;
            }

            return Ok<CustomCommandResponse>();
        }
    }
}
