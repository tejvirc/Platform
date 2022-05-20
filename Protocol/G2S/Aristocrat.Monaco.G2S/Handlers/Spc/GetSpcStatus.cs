namespace Aristocrat.Monaco.G2S.Handlers.Spc
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class GetSpcStatus : ICommandHandler<spc, getSpcStatus>
    {
        private readonly ICommandBuilder<ISpcDevice, spcStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        public GetSpcStatus(IG2SEgm egm, ICommandBuilder<ISpcDevice, spcStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<Error> Verify(ClassCommand<spc, getSpcStatus> command)
        {
            return await Sanction.OwnerAndGuests<IBonusDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<spc, getSpcStatus> command)
        {
            var device = _egm.GetDevice<ISpcDevice>(command.IClass.deviceId);

            // device.NotifyActive();

            var response = command.GenerateResponse<spcStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}
