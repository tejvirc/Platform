namespace Aristocrat.Bingo.Client.Messages.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ServerApiGateway;
    using IMessage = Google.Protobuf.IMessage;

    public class DisableCommandProcessor : ICommandProcessor
    {
        private readonly IMessageHandlerFactory _handlerFactory;

        public DisableCommandProcessor(IMessageHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public async Task<IMessage> ProcessCommand(Command command, CancellationToken token)
        {
            var disableCommand = command.Command_.Unpack<DisableCommand>();
            var message = new Disable(disableCommand.Reason, disableCommand.CashOut);
            var result = await _handlerFactory.Handle<DisableResponse, Disable>(message, token);
            return new DisableCommandResponse
            {
                StatusMessage = command.GetType().Name,
                Status = result.ResponseCode == ResponseCode.Ok
            };
        }
    }
}