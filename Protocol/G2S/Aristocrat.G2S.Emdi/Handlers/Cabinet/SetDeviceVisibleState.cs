namespace Aristocrat.G2S.Emdi.Handlers.Cabinet
{
    using Emdi.Host;
    using Extensions;
    using log4net;
    using Protocol.v21ext1b1;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// The <see cref="setDeviceVisibleState"/> command is sent by media display content to the EGM to show or hide the
    /// media display window.
    /// </summary>
    public class SetDeviceVisibleState : CommandHandler<setDeviceVisibleState>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMediaAdapter _media;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDeviceVisibleState"/> class.
        /// </summary>
        /// <param name="media">An instance of <see cref="IMediaAdapter"/> interface</param>
        public SetDeviceVisibleState(IMediaAdapter media)
        {
            _media = media;
        }

        /// <inheritdoc />
        public override async Task<CommandResult> ExecuteAsync(setDeviceVisibleState command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            if (!_media.Provider.PlayerExist(Context.Config.Port))
            {
                return InvalidXml();
            }

            var status = await _media.SetDeviceVisibleAsync(Context.Config.Port, command.deviceVisibleState);

            return Success(
                new deviceVisibleStatus
                {
                    deviceVisibleState = status
                });
        }
    }
}
