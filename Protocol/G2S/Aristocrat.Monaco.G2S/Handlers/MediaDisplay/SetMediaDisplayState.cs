namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the <see cref="SetMediaDisplayState" /> request from host
    /// </summary>
    public class SetMediaDisplayState : ICommandHandler<mediaDisplay, setMediaDisplayState>
    {
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Constructor for <see cref="SetMediaDisplayState" />
        /// </summary>
        public SetMediaDisplayState(
            IG2SEgm egm,
            ICommandBuilder<IMediaDisplay, mediaDisplayStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, setMediaDisplayState> command)
        {
            return await Sanction.OnlyOwner<IMediaDisplay>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<mediaDisplay, setMediaDisplayState> command)
        {
            var device = _egm.GetDevice<IMediaDisplay>(command.IClass.deviceId);

            SetState(device, command.Command.enable, command.Command.disableText);

            var response = command.GenerateResponse<mediaDisplayStatus>();

            await _commandBuilder.Build(device, response.Command);
        }

        private void SetState(IMediaDisplay device, bool enabled, string message)
        {
            if (device.HostEnabled == enabled)
            {
                return;
            }

            device.DisableText = message;
            device.HostEnabled = enabled;
        }
    }
}
