namespace Aristocrat.G2S.Emdi.Handlers.Comms
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;
    using Monaco.Application.Contracts.Media;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Handles the <see cref="commsOnLine"/> command
    /// </summary>
    [RequiresValidSession(false)]
    public class CommsOnLine : CommandHandler<commsOnLine>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IMediaProvider _media;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommsOnLine"/> class.
        /// </summary>
        public CommsOnLine(
            IMediaProvider media)
        {
            _media = media;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(commsOnLine command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            var player = _media.GetMediaPlayers().FirstOrDefault(x => x.Port == Context.Config.Port);

            if (player == null)
            {
                Context.Session.Status = SessionState.Invalid;
                return Invalid();
            }

            if (player.ActiveMedia == null || command.mdAccessToken != player.ActiveMedia.AccessToken)
            {
                Context.Session.Status = SessionState.Invalid;
                return Invalid();
            }

            Context.Session.Status = SessionState.Valid;

            return await Task.FromResult(Valid());
        }

        private static CommandResult Valid()
        {
            var result = new CommandResult(EmdiErrorCode.NoError);
            result.SetCommand(
                new commsOnLineAck
                {
                    sessionValid = true
                });
            return result;
        }

        private static CommandResult Invalid()
        {
            var result = new CommandResult(EmdiErrorCode.NoError);
            result.SetCommand(
                new commsOnLineAck
                {
                    sessionValid = false
                });
            return result;
        }
    }
}
