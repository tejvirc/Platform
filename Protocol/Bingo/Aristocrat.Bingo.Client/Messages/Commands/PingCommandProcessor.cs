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
            var ping = command.Command_.Unpack<ActivityRequest>();
            return Task.FromResult<IMessage>(
                new ActivityResponse()
                {
                    //ActivityTime = ping.PingRequestTime,
                    ActivityResponseTime = Timestamp.FromDateTime(DateTime.UtcNow)
                });
        }
    }
}