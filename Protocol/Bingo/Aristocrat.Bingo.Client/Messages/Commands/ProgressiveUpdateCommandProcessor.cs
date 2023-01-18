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

    public class ProgressiveUpdateCommandProcessor : IProgressiveCommandProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IMessageHandlerFactory _handlerFactory;

        public ProgressiveUpdateCommandProcessor(IMessageHandlerFactory handlerFactory)
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
                return null;
            }
            else if (command.ProgressiveMeta.Is(EnableByProgressive.Descriptor))
            {
                Logger.Debug("ENABLE from progressive controller");
                var enableByProgressive = new EnableByProgressiveMessage(ResponseCode.Ok);
                await _handlerFactory
                    .Handle<ProgressiveUpdateResponse, EnableByProgressiveMessage>(enableByProgressive, token)
                    .ConfigureAwait(false);
                return null;
            }
            else
            {
                var update = command.ProgressiveMeta.Unpack<ProgressiveLevelUpdate>();
                var progressiveUpdate = new ProgressiveUpdateMessage(
                    ResponseCode.Ok,
                    update.ProgressiveLevel,
                    update.NewValue);

                Logger.Debug($"ProgressiveLevelUpdate: progLevel={update.ProgressiveLevel}, newValue={update.NewValue}");

                await _handlerFactory
                    .Handle<ProgressiveUpdateResponse, ProgressiveUpdateMessage>(progressiveUpdate, token)
                    .ConfigureAwait(false);
                return null;
            }
        }
    }
}