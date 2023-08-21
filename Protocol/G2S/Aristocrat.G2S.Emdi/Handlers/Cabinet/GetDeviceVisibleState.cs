namespace Aristocrat.G2S.Emdi.Handlers.Cabinet
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;
    using log4net;
    using Monaco.Application.Contracts.Media;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Handles the <see cref="getDeviceVisibleState"/> command.
    /// </summary>
    public class GetDeviceVisibleState : CommandHandler<getDeviceVisibleState>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMediaProvider _media;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetDeviceVisibleState"/> class.
        /// </summary>
        /// <param name="media"></param>
        public GetDeviceVisibleState(IMediaProvider media)
        {
            _media = media;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(getDeviceVisibleState command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            var player = _media.GetMediaPlayers().FirstOrDefault(x => x.Port == Context.Config.Port);

            if (player == null)
            {
                return InvalidXml();
            }

            return await Task.FromResult(Success(
                new deviceVisibleStatus
                {
                    deviceVisibleState = player.Visible
                }));
        }
    }
}
