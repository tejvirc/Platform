namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Session;

    public class GetPlayerProfile : ICommandHandler<player, getPlayerProfile>
    {
        private readonly IPlayerService _players;
        private readonly IG2SEgm _egm;
        private readonly IPlayerSessionHistory _sessions;

        public GetPlayerProfile(IG2SEgm egm, IPlayerSessionHistory sessions, IPlayerService players)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            _players = players ?? throw new ArgumentNullException(nameof(players));
        }

        public async Task<Error> Verify(ClassCommand<player, getPlayerProfile> command)
        {
            return await Sanction.OwnerAndGuests<IPlayerDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<player, getPlayerProfile> command)
        {
            var device = _egm.GetDevice<IPlayerDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<playerProfile>();

            response.Command.configurationId = device.ConfigurationId;
            response.Command.configDateTime = device.ConfigDateTime;
            response.Command.configComplete = device.ConfigComplete;
            response.Command.restartStatus = device.RestartStatus;
            response.Command.useDefaultConfig = device.UseDefaultConfig;
            response.Command.requiredForPlay = device.RequiredForPlay;
            response.Command.minLogEntries = _sessions.MaxEntries;
            response.Command.timeToLive = (int)device.Queue.SessionTimeout.TotalMilliseconds;
            response.Command.displayPresent = false;

            response.Command.idReaderId = device.IdReader;

            response.Command.minTheoHoldPct = _players.Options.MinimumTheoreticalHoldPercentageMeter;
            response.Command.decimalPoints = _players.Options.DecimalPoints;

            response.Command.inactiveSessionEnd = _players.Options.InactiveSessionEnd;
            response.Command.intervalPeriod = (int)_players.Options.IntervalPeriod.TotalMilliseconds;
            response.Command.gamePlayInterval = _players.Options.GamePlayInterval;
            response.Command.msgDuration = device.MessageDuration;
            response.Command.countBasis = _players.Options.CountBasis;
            response.Command.countDirection = _players.Options.CountDirection == CountDirection.Up
                ? t_countDirection.G2S_up
                : t_countDirection.G2S_down;
            response.Command.baseTarget = _players.Options.BaseTarget;
            response.Command.baseAward = _players.Options.BaseAward;
            response.Command.baseIncrement = _players.Options.BaseIncrement;
            response.Command.hotPlayerBasis = _players.Options.HotPlayerBasis;
            response.Command.hotPlayerPeriod = (int)_players.Options.HotPlayerPeriod.TotalMilliseconds;
            response.Command.hotPlayerLimit1 = _players.Options.HotPlayerLimit1;
            response.Command.hotPlayerLimit2 = _players.Options.HotPlayerLimit2;
            response.Command.hotPlayerLimit3 = _players.Options.HotPlayerLimit3;
            response.Command.hotPlayerLimit4 = _players.Options.HotPlayerLimit4;
            response.Command.hotPlayerLimit5 = _players.Options.HotPlayerLimit5;

            response.Command.meterDeltaSupported = device.MeterDeltaSupported;
            response.Command.sendMeterDelta = device.SendMeterDelta;

            response.Command.useMultipleIdDevices = device.UseMultipleIdDevices;

            await Task.CompletedTask;
        }
    }
}
