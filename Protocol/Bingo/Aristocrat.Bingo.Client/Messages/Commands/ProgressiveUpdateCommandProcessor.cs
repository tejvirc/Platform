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
        private readonly IProgressiveMessageHandlerFactory _handlerFactory;

        public ProgressiveUpdateCommandProcessor(IProgressiveMessageHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        public async Task<IMessage> ProcessCommand(ProgressiveUpdate command, CancellationToken token)
        {
            var update = command.ProgressiveMeta.Unpack<ProgressiveLevelUpdate>();
            var progressiveUpdate = new ProgressiveUpdateMessage(
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