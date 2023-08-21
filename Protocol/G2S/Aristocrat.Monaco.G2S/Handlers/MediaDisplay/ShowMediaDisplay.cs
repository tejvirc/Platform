namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Message handler for <see cref="showMediaDisplay"/> request from host
    /// </summary>
    public class ShowMediaDisplay : ICommandHandler<mediaDisplay, showMediaDisplay>
    {
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayAck> _command;
        private readonly IG2SEgm _egm;
        private readonly IMediaProvider _mediaProvider;

        private IMediaDisplay _device;
        private IMediaPlayer _player;

        /// <summary>
        ///     Constructor for <see cref="ShowMediaDisplay"/>
        /// </summary>
        /// <param name="egm">an <see cref="IG2SEgm"/> object</param>
        /// <param name="command">a command builder for the <see cref="mediaDisplayAck"/> response</param>
        /// <param name="mediaProvider">the <see cref="IMediaProvider"/> object</param>
        public ShowMediaDisplay(
            IG2SEgm egm,
            ICommandBuilder<IMediaDisplay, mediaDisplayAck> command,
            IMediaProvider mediaProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, showMediaDisplay> command)
        {
            var error = await Sanction.OnlyOwner<IMediaDisplay>(_egm, command);
            if (error != null)
            {
                return error;
            }

            _device = _egm.GetDevice<IMediaDisplay>(command.IClass.deviceId);

            _player = _mediaProvider.GetMediaPlayer(_device.Id);

            if (null == _player.ActiveMedia || _player.ActiveMedia.IsFinalized)
            {
                return new Error(ErrorCode.IGT_MDX004);
            }

            if (_player.ActiveMedia.EmdiConnectionRequired && !_player.EmdiConnected)
            {
                return new Error(ErrorCode.G2S_MDX009);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<mediaDisplay, showMediaDisplay> command)
        {
            _mediaProvider.Show(command.Class.deviceId);

            var response = command.GenerateResponse<mediaDisplayAck>();

            await _command.Build(_device, response.Command);

            // Override visible state because we are queuing states and want to return the correct expected state
            response.Command.deviceVisibleState = t_deviceVisibleStates.IGT_shown;

            // Response events occur upon receipt of ShowMediaPlayerEvent elsewhere.
        }
    }
}
