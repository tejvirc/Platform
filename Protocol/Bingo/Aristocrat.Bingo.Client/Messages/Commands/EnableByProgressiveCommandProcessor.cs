namespace Aristocrat.Bingo.Client.Messages.Commands
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using log4net;
    using Progressives;
    using ServerApiGateway;
    using IMessage = Google.Protobuf.IMessage;

    public class EnableByProgressiveCommandProcessor : IProgressiveCommandProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IProgressiveMessageHandlerFactory _handlerFactory;

        public EnableByProgressiveCommandProcessor(IProgressiveMessageHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public async Task<IMessage> ProcessCommand(ProgressiveUpdate command, CancellationToken token)
        {
            if (command.ProgressiveMeta.Is(EnableByProgressive.Descriptor))
            {
                Logger.Debug("ENABLE from progressive controller");
                var enableByProgressive = new EnableByProgressiveMessage(ResponseCode.Ok);
                await _handlerFactory
                    .Handle<ProgressiveUpdateResponse, EnableByProgressiveMessage>(enableByProgressive, token)
                    .ConfigureAwait(false);
            }

            return null;
        }
    }
}