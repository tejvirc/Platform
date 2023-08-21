namespace Aristocrat.Monaco.G2S.Handlers.Spc
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class GetSpcProfile : ICommandHandler<spc, getSpcProfile>
    {
        private readonly IG2SEgm _egm;

        public GetSpcProfile(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        public async Task<Error> Verify(ClassCommand<spc, getSpcProfile> command)
        {
            return await Sanction.OwnerAndGuests<ISpcDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<spc, getSpcProfile> command)
        {
            var device = _egm.GetDevice<ISpcDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<spcProfile>();

            response.Command.configurationId = device.ConfigurationId;
            response.Command.restartStatus = device.RestartStatus;
            response.Command.requiredForPlay = device.RequiredForPlay;
            response.Command.minLogEntries = device.MinLogEntries;
            response.Command.timeToLive = device.TimeToLive;
            response.Command.controllerType = "G2S_standaloneProg";
            response.Command.configComplete = device.ConfigComplete;
            response.Command.configDateTime = device.ConfigDateTime;
            response.Command.useDefaultConfig = false;
            // TODO: populate response.Command.spcLevelProfile

            await Task.CompletedTask;
        }
    }
}
