namespace Aristocrat.Monaco.G2S.Handlers.InformedPlayer
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handle the <see cref="getIpProfile" /> command
    /// </summary>
    public class GetIpProfile : ICommandHandler<informedPlayer, getIpProfile>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Constructor for <see cref="GetIpProfile" />
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> object</param>
        public GetIpProfile(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<informedPlayer, getIpProfile> command)
        {
            return await Sanction.OwnerAndGuests<IInformedPlayerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<informedPlayer, getIpProfile> command)
        {
            var device = _egm.GetDevice<IInformedPlayerDevice>(command.IClass.deviceId);
            device.HostActive = true;

            var response = command.GenerateResponse<ipProfile>();

            response.Command.configurationId = device.ConfigurationId;
            response.Command.configDateTime = device.ConfigDateTime;
            response.Command.configComplete = device.ConfigComplete;
            response.Command.restartStatus = device.RestartStatus;
            response.Command.useDefaultConfig = device.UseDefaultConfig;
            response.Command.requiredForPlay = device.RequiredForPlay;
            response.Command.timeToLive = device.TimeToLive;

            response.Command.noMessageTimer = device.NoMessageTimer;
            response.Command.noHostText = device.NoHostText;
            response.Command.uncardedMoneyIn = device.UnCardedMoneyIn;
            response.Command.uncardedGamePlay = device.UnCardedGamePlay;
            response.Command.sessionStartMoneyIn = device.SessionStartMoneyIn;
            response.Command.sessionStartGamePlay = device.SessionStartGamePlay;
            response.Command.sessionStartCashOut = device.SessionStartCashOut;
            response.Command.sessionEndCashOut = device.SessionEndCashOut;
            response.Command.sessionStartPinEntry = device.SessionStartPinEntry;
            response.Command.sessionStartLimit = device.SessionStartLimit;
            response.Command.authenticationDevice = new authenticationDevice[0];

            await Task.CompletedTask;
        }
    }
}