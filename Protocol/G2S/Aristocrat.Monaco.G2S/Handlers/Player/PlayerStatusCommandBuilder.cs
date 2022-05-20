namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Session;

    public class PlayerStatusCommandBuilder : ICommandBuilder<IPlayerDevice, playerStatus>
    {
        private readonly IPlayerService _playerService;

        public PlayerStatusCommandBuilder(IPlayerService playerService)
        {
            _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        }

        public async Task Build(IPlayerDevice device, playerStatus command)
        {
            command.configurationId = device.ConfigurationId;

            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;

            var genericOverride = _playerService.GetParameters<GenericOverrideParameters>();
            if (genericOverride != null)
            {
                command.overrideId = genericOverride.OverrideId;
                command.overrideStartSpecified = true;
                command.overrideStart = genericOverride.Start;
                command.overrideEndSpecified = true;
                command.overrideEnd = genericOverride.End;
                command.overrideTarget = genericOverride.Target;
                command.overrideIncrement = genericOverride.Increment;
                command.overrideAward = genericOverride.Award;
            }
            else
            {
                command.overrideStartSpecified = false;
                command.overrideEndSpecified = false;
            }

            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;

            if (_playerService.HasActiveSession)
            {
                command.pointBalanceSpecified = true;
                command.pointBalance = _playerService.ActiveSession.PointBalance;
                command.playerStartSpecified = true;
                command.playerStart = _playerService.ActiveSession.Start;
                command.playerEndSpecified = true;
                command.playerEnd = _playerService.ActiveSession.End;

                var sessionOverride = _playerService.GetParameters<SessionParameters>();
                if (sessionOverride != null)
                {
                    command.playerTarget = sessionOverride.Target;
                    command.playerIncrement = sessionOverride.Increment;
                    command.playerAward = sessionOverride.Award;
                }
            }
            else
            {
                command.pointBalanceSpecified = false;
                command.playerStartSpecified = false;
                command.playerEndSpecified = false;
            }

            await Task.CompletedTask;
        }
    }
}
