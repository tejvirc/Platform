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
    ///     Handle the <see cref="hideMediaDisplay"/> command
    /// </summary>
    [ProhibitWhenDisabled]
    public class HideMediaDisplay : ICommandHandler<mediaDisplay, hideMediaDisplay>
    {
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayAck> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IMediaProvider _mediaProvider;

        /// <summary>
        ///     Constructor for <see cref="HideMediaDisplay"/>
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm"/> object</param>
        /// <param name="commandBuilder">A command builder for <see cref="mediaDisplayAck"/> response</param>
        /// <param name="mediaProvider">The <see cref="IMediaProvider"/> object</param>
        public HideMediaDisplay(
            IG2SEgm egm,
            ICommandBuilder<IMediaDisplay, mediaDisplayAck> commandBuilder,
            IMediaProvider mediaProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _mediaProvider = mediaProvider ?? throw new ArgumentNullException(nameof(mediaProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, hideMediaDisplay> command)
        {
            return await Sanction.OnlyOwner<IMediaDisplay>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<mediaDisplay, hideMediaDisplay> command)
        {
            var device = _egm.GetDevice<IMediaDisplay>(command.IClass.deviceId);

            _mediaProvider.Hide(command.Class.deviceId);

            var response = command.GenerateResponse<mediaDisplayAck>();

            await _commandBuilder.Build(device, response.Command);

            // Override visible state because we are queuing states and want to return the correct expected state
            response.Command.deviceVisibleState = t_deviceVisibleStates.IGT_hidden;

            // Response events occur upon receipt of HideMediaPlayerEvent elsewhere.
        }
    }
}
