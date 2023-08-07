namespace Aristocrat.G2S.Emdi.Handlers.ContentToContent
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;
    using Monaco.Application.Contracts.Media;
    using Monaco.Common;
    using Protocol.v21ext1b1;

    /// <summary>
    ///     Handles the <see cref="contentMessage"/> command
    /// </summary>
    public class ContentMessage : CommandHandler<contentMessage>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IMediaProvider _media;
        private readonly IHostQueue _server;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentMessage"/> class.
        /// </summary>
        public ContentMessage(
            IMediaProvider media,
            IHostQueue server)
        {
            _media = media;
            _server = server;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(contentMessage command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            try
            {
                var target = _media.GetMediaPlayer(command.mediaDisplayId);

                if (target == null)
                {
                    return InvalidXml();
                }

                if (target.ActiveMedia?.Id != command.contentId)
                {
                    return InterfaceNotOpen();
                }

                var source = _media.GetMediaPlayers().FirstOrDefault(x => x.Port == Context.Config.Port);

                var contentToken = source?.ActiveMedia?.MdContentToken ?? 0;

                if (contentToken == 0 || target.ActiveMedia?.MdContentToken != contentToken)
                {
                    return ContentToContentNotPermitted();
                }

                Task.Run(async () => await _server[target.Port].SendCommandAsync<mdContentToContent, contentMessage, contentMessageAck>(
                    new contentMessage
                    {
                        mediaDisplayId = command.mediaDisplayId,
                        contentId = command.contentId,
                        contentData = command.contentData
                    }))
                    .FireAndForget(ex => LogException(ex, Context.Config.Port, target.Port));
            }
            catch (Exception ex)
            {
                LogException(ex, Context.Config.Port, command.mediaDisplayId + 1023);
                return InterfaceNotOpen();
            }

            return await Task.FromResult(Success(new contentMessageAck()));
        }

        private static void LogException(Exception ex, int sourcePort, int targetPort)
        {
            var exception = ex is AggregateException aex ? aex.InnerException : ex;

            if (exception is MessageException mex)
            {
                Logger.Error($"EMDI: Error ({mex.ErrorCode}) sending content message from {sourcePort} to {targetPort}: {mex.Message}", exception);
            }
            else
            {
                Logger.Error($"EMDI: Error sending content message from {sourcePort} to {targetPort}: {exception?.Message}", exception);
            }
        }
    }
}
