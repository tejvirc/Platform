namespace Aristocrat.Monaco.G2S.Handlers.Central
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class GetCentralStatus : ICommandHandler<central, getCentralStatus>
    {
        private readonly ICommandBuilder<ICentralDevice, centralStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        public GetCentralStatus(IG2SEgm egm, ICommandBuilder<ICentralDevice, centralStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<Error> Verify(ClassCommand<central, getCentralStatus> command)
        {
            return await Sanction.OwnerAndGuests<ICentralDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<central, getCentralStatus> command)
        {
            var device = _egm.GetDevice<ICentralDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<centralStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}