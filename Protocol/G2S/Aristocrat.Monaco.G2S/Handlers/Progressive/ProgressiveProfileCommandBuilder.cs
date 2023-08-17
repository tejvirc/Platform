namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class ProgressiveProfileCommandBuilder : ICommandBuilder<IProgressiveDevice, progressiveProfile>
    {
        private readonly ITransactionHistory _transactions;
        private readonly IGameProvider _gameProvider;
        private readonly IProgressiveLevelProvider _progressives;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        public ProgressiveProfileCommandBuilder(
            IProgressiveLevelProvider progressiveProvider,
            ITransactionHistory transactions,
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _progressives = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
        }

        /// <inheritdoc />
        public async Task Build(IProgressiveDevice device, progressiveProfile command)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var linkedLevels = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Where(ll => ll.ProgressiveGroupId == device.ProgressiveId && ll.ProtocolName == ProtocolNames.G2S).ToList();
            var linkedLevelNames = linkedLevels.Select(ll => ll.LevelName).ToList();
            var levels = _progressives.GetProgressiveLevels()
                .Where(l => linkedLevelNames.Contains(l.AssignedProgressiveId?.AssignedProgressiveKey ?? string.Empty))
                .GroupBy(l => l.AssignedProgressiveId.AssignedProgressiveKey)
                .ToDictionary(group => group.Key, group => group.ToList());

            command.configurationId = device.ConfigurationId;
            command.minLogEntries = _transactions.GetMaxTransactions<JackpotTransaction>();
            command.noProgInfo = device.NoProgressiveInfo;
            command.noResponseTimer = (int)device.NoResponseTimer.TotalMilliseconds;
            command.progId = device.ProgressiveId;
            command.requiredForPlay = device.RequiredForPlay;
            command.restartStatus = device.RestartStatus;
            command.timeToLive = device.TimeToLive;
            command.useDefaultConfig = device.UseDefaultConfig;
            command.configComplete = device.ConfigComplete;
            command.configDateTime = device.ConfigDateTime;

            var levelProfiles = new List<levelProfile>();
            foreach(var linkedLevel in linkedLevels)
            {
                if (levelProfiles.Any(p => p.levelId == linkedLevel.LevelId))
                    continue;

                var progLevels = levels[linkedLevel.LevelName];
                var firstProgLevel = progLevels.First();

                var profile = new levelProfile();
                profile.levelId = linkedLevel.LevelId;
                var levelType = firstProgLevel.FundingType.ToString().ToLower();
                profile.levelType = $"{Constants.ManufacturerPrefix}_{levelType}";
                profile.incrementRate = firstProgLevel.IncrementRate;
                profile.maxValue = firstProgLevel.MaximumValue;
                profile.resetValue = firstProgLevel.ResetValue;

                var gameLevelConfigs = new List<gameLevelConfig>();
                foreach (var progLevel in progLevels)
                {
                    var game = _gameProvider.GetGame(progLevel.GameId);
                    foreach (var denom in game.Denominations)
                    {
                        if (!denom.Active)
                            continue;
                        
                        var config = new gameLevelConfig();
                        config.denomId = denom.Id;
                        config.gamePlayId = progLevel.GameId;
                        config.numberOfCredits = denom.MinimumWagerCredits < 1 ? 1 : denom.MinimumWagerCredits;
                        config.paytableId = game.PaytableId;
                        config.themeId = game.ThemeId;
                        config.turnover = progLevel.Turnover;
                        config.linkThemeId = game.ThemeId;
                        config.winLevelCombo = progLevel.LevelName;
                        config.winLevelIndex = progLevel.LevelId; //May need updated in the future to match vertex levelIds
                        config.winLevelOdds = progLevel.LevelRtp;
                        config.variation = int.Parse(game.VariationId);
                        gameLevelConfigs.Add(config);
                    }
                }
                profile.gameLevelConfig = gameLevelConfigs.ToArray();

                levelProfiles.Add(profile);
            }
            command.levelProfile = levelProfiles.ToArray();

            if (command.levelProfile?.Length == 0)
            {
                command.levelProfile = new levelProfile[1];
                command.levelProfile[0] = new levelProfile { levelId = 0, gameLevelConfig = Array.Empty<gameLevelConfig>() };
            }

            await Task.CompletedTask;
        }
    }
}