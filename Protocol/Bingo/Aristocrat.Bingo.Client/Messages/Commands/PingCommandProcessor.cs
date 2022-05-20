namespace Aristocrat.Bingo.Client.Messages.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Protobuf.WellKnownTypes;
    using ServerApiGateway;
    using IMessage = Google.Protobuf.IMessage;

    public class PingCommandProcessor : ICommandProcessor
    {
        public Task<IMessage> ProcessCommand(Command command, CancellationToken token)
        {
            var ping = command.Command_.Unpack<PingCommand>();
            return Task.FromResult<IMessage>(
                new PingResponse
                {
                    PingRequestTime = ping.PingRequestTime,
                    PingResponseTime = Timestamp.FromDateTime(DateTime.UtcNow)
                });
        }
    }
}