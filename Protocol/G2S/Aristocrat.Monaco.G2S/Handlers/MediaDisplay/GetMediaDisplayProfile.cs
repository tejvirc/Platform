namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    ///     Handle the <see cref="getMediaDisplayProfile"/> command
    /// </summary>
    public class GetMediaDisplayProfile : ICommandHandler<mediaDisplay, getMediaDisplayProfile>
    {
        private readonly IG2SEgm _egm;
        private readonly IMediaProvider _mediaProvider;

        /// <summary>
        ///     Constructor for <see cref="GetMediaDisplayProfile"/>
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm"/> object</param>
        /// <param name="mediaProvider">The <see cref="IMediaProvider"/> object</param>
        public GetMediaDisplayProfile(IG2SEgm egm, IMediaProvider mediaProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, getMediaDisplayProfile> command)
        {
            return await Sanction.OwnerAndGuests<IMediaDisplay>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<mediaDisplay, getMediaDisplayProfile> command)
        {
            var device = _egm.GetDevice<IMediaDisplay>(command.IClass.deviceId);

            var response = command.GenerateResponse<mediaDisplayProfile>();

            var mediaPlayer = _mediaProvider.GetMediaPlayer(device.Id);

            response.Command.configurationId = device.ConfigurationId;
            response.Command.restartStatus = device.RestartStatus;
            response.Command.requiredForPlay = device.RequiredForPlay;
            response.Command.minLogEntries = _mediaProvider.MinimumMediaLogSize;
            response.Command.timeToLive = device.TimeToLive;

            response.Command.mediaDisplayPriority = mediaPlayer.Priority;
            response.Command.screenType = mediaPlayer.ScreenType.ToProtocolType();
            response.Command.screenDescription = mediaPlayer.ScreenDescription;
            response.Command.mediaDisplayType = mediaPlayer.DisplayType.ToProtocolType();
            response.Command.mediaDisplayPosition = mediaPlayer.DisplayPosition.ToProtocolType();
            response.Command.mediaDisplayDescription = mediaPlayer.Description;

            response.Command.maxContentLoaded = _mediaProvider.MaxMediaStorage;

            response.Command.xPosition = mediaPlayer.XPosition;
            response.Command.yPosition = mediaPlayer.YPosition;
            response.Command.contentHeight = mediaPlayer.Height;
            response.Command.contentWidth = mediaPlayer.Width;
            response.Command.mediaDisplayHeight = mediaPlayer.DisplayHeight;
            response.Command.mediaDisplayWidth = mediaPlayer.DisplayWidth;
            response.Command.touchscreenCapable = mediaPlayer.TouchCapable;
            response.Command.localConnectionPort = mediaPlayer.Port;
            response.Command.audioCapable = mediaPlayer.AudioCapable;
            response.Command.nativeResSupported = true;
            response.Command.emdiConReqSupported = true;

            response.Command.configDateTime = device.ConfigDateTime;
            response.Command.configComplete = device.ConfigComplete;
            response.Command.useDefaultConfig = device.UseDefaultConfig;

            response.Command.capabilitiesList1 = MediaDisplayExtensions.ImplementedCapabilitiesList();

            response.Command.localCommandList1 = MediaDisplayExtensions.ImplementedCommandList();

            response.Command.localEventList1 = MediaDisplayExtensions.ImplementedEventList();

            response.Command.screenList1 = new c_mediaDisplayProfile.screenList
            {
                screenItem1 = _mediaProvider.GetScreens().Select(
                    s => new c_mediaDisplayProfile.screenList.screenItem
                    {
                        screenDescription = s.Description,
                        screenHeight = s.Height,
                        screenType = s.Type.ToProtocolType(),
                        screenWidth = s.Width
                    }).ToArray()
            };

            await Task.CompletedTask;
        }
    }
}
