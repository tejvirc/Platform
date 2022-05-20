namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Handle the <see cref="getMediaDisplayStatus"/> command
    /// </summary>
    public class GetMediaDisplayStatus : ICommandHandler<mediaDisplay, getMediaDisplayStatus>
    {
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Constructor for <see cref="GetMediaDisplayStatus"/>
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm"/> object</param>
        /// <param name="commandBuilder">A command builder for <see cref="mediaDisplayStatus"/> response</param>
        public GetMediaDisplayStatus(
            IG2SEgm egm,
            ICommandBuilder<IMediaDisplay, mediaDisplayStatus> commandBuilder)

        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, getMediaDisplayStatus> command)
        {
            return await Sanction.OwnerAndGuests<IMediaDisplay>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<mediaDisplay, getMediaDisplayStatus> command)
        {
            var device = _egm.GetDevice<IMediaDisplay>(command.IClass.deviceId);

            var response = command.GenerateResponse<mediaDisplayStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}
