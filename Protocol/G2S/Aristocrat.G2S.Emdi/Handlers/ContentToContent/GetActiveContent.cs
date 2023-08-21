namespace Aristocrat.G2S.Emdi.Handlers.ContentToContent
{
    using Monaco.Application.Contracts.Media;
    using Protocol.v21ext1b1;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;

    /// <summary>
    ///     Handles the <see cref="getActiveContent"/> command
    /// </summary>
    public class GetActiveContent : CommandHandler<getActiveContent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMediaProvider _media;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetActiveContent"/> class.
        /// </summary>
        public GetActiveContent(
            IMediaProvider media)
        {
            _media = media;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(getActiveContent command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            var player = _media.GetMediaPlayers().FirstOrDefault(x => x.Port == Context.Config.Port);

            if (player == null)
            {
                return Empty();
            }

            var contentToken = player.ActiveMedia?.MdContentToken ?? 0;

            if (contentToken == 0)
            {
                return Empty();
            }

            return await Task.FromResult(Success(
                new activeContentList
                {
                    activeContent = _media.GetMediaPlayers().Where(p => p.ActiveMedia?.MdContentToken == contentToken)
                        .Select(
                            p => new activeContent
                            {
                                contentId = p.ActiveMedia.Id,
                                mediaDisplayId = p.Id
                            })
                        .ToArray()
                }));
        }

        private static CommandResult Empty()
        {
            var result = new CommandResult(EmdiErrorCode.NoError);
            result.SetCommand(new activeContentList { activeContent = new activeContent[] { } });
            return result;
        }
    }
}
