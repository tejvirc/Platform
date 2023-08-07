namespace Aristocrat.G2S.Emdi.Handlers.Cabinet
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
    ///     Handles the <see cref="contentToHostMessage"/> command
    /// </summary>
    public class ContentToHostMessage : CommandHandler<contentToHostMessage>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IMediaProvider _media;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentToHostMessage"/> class.
        /// </summary>
        /// <param name="media"></param>
        public ContentToHostMessage(
            IMediaProvider media)
        {
            _media = media;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(contentToHostMessage command)
        {
            try
            {
                Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

                var player = _media.GetMediaPlayers().FirstOrDefault(x => x.Port == Context.Config.Port);

                if (player == null)
                {
                    return InvalidXml();
                }

                if (command.instructionData?.Value == null)
                {
                    return InvalidXml();
                }

                Task.Run(
                        () => _media.SendEmdiFromContentToHost(
                            player.Id,
                            Convert.ToBase64String(command.instructionData.Value)))
                    .FireAndForget(ex => LogException(ex, Context.Config.Port));
            }
            catch (Exception ex)
            {
                LogException(ex, Context.Config.Port);
            }

            return await Task.FromResult(Success(new contentToHostMessageAck()));
        }

        private static void LogException(Exception ex, int port)
        {
            var exception = ex is AggregateException aex ? aex.InnerExceptions.FirstOrDefault() : ex;

            Logger.Error($"EMDI: Error handling content-to-host command on port {port}", exception);
        }
    }
}
