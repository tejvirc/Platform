namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.setDownloadState G2S message
    /// </summary>
    public class SetDownloadState : ICommandHandler<download, setDownloadState>
    {
        private readonly ICommandBuilder<IDownloadDevice, downloadStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetDownloadState" /> class.
        /// </summary>
        public SetDownloadState(
            IG2SEgm egm,
            ICommandBuilder<IDownloadDevice, downloadStatus> commandBuilder)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, setDownloadState> command)
        {
            return await Sanction.OnlyOwner<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, setDownloadState> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (device == null)
                {
                    return;
                }

                var enabled = command.Command.enable;

                if (device.HostEnabled != enabled)
                {
                    // TODO: check pending trans and running scripts
                    // let scripts complete to next step then pause
                    // let package command proceed to next step then pause or abort (depending on command)
                    device.DisableText = command.Command.disableText;
                    device.HostEnabled = enabled;
                }

                var response = command.GenerateResponse<downloadStatus>();

                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}
