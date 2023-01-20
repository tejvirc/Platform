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

    public class DisableProgressiveCommandProcessor : IProgressiveCommandProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IMessageHandlerFactory _handlerFactory;

        public DisableProgressiveCommandProcessor(IMessageHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public async Task<IMessage> ProcessCommand(ProgressiveUpdate command, CancellationToken token)
        {
            if (command.ProgressiveMeta.Is(DisableByProgressive.Descriptor))
            {
                Logger.Debug("DISABLE from progressive controller");

                var disableByProgressive = new DisableByProgressiveMessage(ResponseCode.Ok);
                await _handlerFactory
                    .Handle<ProgressiveUpdateResponse, DisableByProgressiveMessage>(disableByProgressive, token)
                    .ConfigureAwait(false);
            }

            return null;
        }
    }
}