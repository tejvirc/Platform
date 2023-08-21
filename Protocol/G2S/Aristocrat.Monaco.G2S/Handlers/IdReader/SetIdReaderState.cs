namespace Aristocrat.Monaco.G2S.Handlers.IdReader
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class SetIdReaderState : ICommandHandler<idReader, setIdReaderState>
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        public SetIdReaderState(
            IG2SEgm egm,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<Error> Verify(ClassCommand<idReader, setIdReaderState> command)
        {
            return await Sanction.OnlyOwner<IIdReaderDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<idReader, setIdReaderState> command)
        {
            var device = _egm.GetDevice<IIdReaderDevice>(command.IClass.deviceId);

            SetState(device, command.Command.enable, command.Command.disableText);

            var response = command.GenerateResponse<idReaderStatus>();

            await _commandBuilder.Build(device, response.Command);
        }

        private void SetState(IIdReaderDevice device, bool enabled, string message)
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
