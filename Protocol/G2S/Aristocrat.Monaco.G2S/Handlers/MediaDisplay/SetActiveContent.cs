namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Media;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handle the <see cref="setActiveContent" /> message
    /// </summary>
    [ProhibitWhenDisabled]
    public class SetActiveContent : ICommandHandler<mediaDisplay, setActiveContent>
    {
        private readonly ICommandBuilder<IMediaDisplay, contentStatus> _command;
        private readonly IG2SEgm _egm;
        private readonly IMediaProvider _mediaProvider;

        private IMediaDisplay _device;
        private IMedia _media;

        /// <summary>
        ///     Constructor for <see cref="SetActiveContent" />
        /// </summary>
        /// <param name="egm">A <see cref="IG2SEgm" /> object</param>
        /// <param name="command">A command builder for <see cref="contentStatus" /> response</param>
        /// <param name="mediaProvider">The <see cref="IMediaProvider" /> object</param>
        public SetActiveContent(
            IG2SEgm egm,
            ICommandBuilder<IMediaDisplay, contentStatus> command,
            IMediaProvider mediaProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, setActiveContent> command)
        {
            var error = await Sanction.OnlyOwner<IMediaDisplay>(_egm, command);
            if (error != null)
            {
                return error;
            }

            _device = _egm.GetDevice<IMediaDisplay>(command.IClass.deviceId);

            _media = _mediaProvider.GetMedia(_device.Id, command.Command.contentId, command.Command.transactionId);

            if (null == _media)
            {
                return new Error(ErrorCode.IGT_MDX002);
            }

            if (MediaState.Pending != _media.State && MediaState.Loaded != _media.State &&
                MediaState.Executing != _media.State)
            {
                return new Error(ErrorCode.IGT_MDX005);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<mediaDisplay, setActiveContent> command)
        {
            _mediaProvider.ActivateContent(
                command.Class.deviceId,
                command.Command.contentId,
                command.Command.transactionId);

            var response = command.GenerateResponse<contentStatus>();

            response.Command.transactionId = command.Command.transactionId;
            response.Command.contentId = command.Command.contentId;

            await _command.Build(_device, response.Command);

            // Response events occur upon receipt of MediaPlayerContentReadyEvent elsewhere.
        }
    }
}