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
    ///     Handle the <see cref="getContentStatus" /> command
    /// </summary>
    [ProhibitWhenDisabled]
    public class GetContentStatus : ICommandHandler<mediaDisplay, getContentStatus>
    {
        private readonly ICommandBuilder<IMediaDisplay, contentStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IMediaProvider _mediaProvider;

        private IMediaDisplay _device;
        private IMedia _media;

        /// <summary>
        ///     Constructor for <see cref="GetContentLogStatus" />
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> object</param>
        /// <param name="commandBuilder">A command builder for <see cref="contentStatus" /> response</param>
        /// <param name="mediaProvider">The <see cref="IMediaProvider" /> object</param>
        public GetContentStatus(
            IG2SEgm egm,
            ICommandBuilder<IMediaDisplay, contentStatus> commandBuilder,
            IMediaProvider mediaProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, getContentStatus> command)
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

            return null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<mediaDisplay, getContentStatus> command)
        {
            var response = command.GenerateResponse<contentStatus>();

            response.Command.transactionId = command.Command.transactionId;
            response.Command.contentId = command.Command.contentId;

            await _commandBuilder.Build(_device, response.Command);
        }
    }
}