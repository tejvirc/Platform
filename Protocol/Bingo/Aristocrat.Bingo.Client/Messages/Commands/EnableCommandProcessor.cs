namespace Aristocrat.Bingo.Client.Messages.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ServerApiGateway;
    using IMessage = Google.Protobuf.IMessage;

    public class EnableCommandProcessor : ICommandProcessor
    {
        private readonly IMessageHandlerFactory _handlerFactory;

        public EnableCommandProcessor(IMessageHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public async Task<IMessage>  ProcessCommand(Command command, CancellationToken token)
        {
            var result = await _handlerFactory.Handle<EnableResponse, Enable>(new Enable(), token);
            return new EnableCommandResponse
            {
                StatusMessage = command.GetType().Name,
                Status = result.ResponseCode == ResponseCode.Ok
            };
        }
    }
}