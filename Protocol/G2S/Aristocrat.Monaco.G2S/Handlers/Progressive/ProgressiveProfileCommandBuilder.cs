namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Services.Progressive;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class ProgressiveProfileCommandBuilder : ICommandBuilder<IProgressiveDevice, progressiveProfile>
    {
        private readonly ITransactionHistory _transactions;
        private readonly IGameProvider _gameProvider;
        private readonly IProgressiveLevelProvider _progressives;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IProgressiveDeviceManager _progressiveDeviceManager;

        public ProgressiveProfileCommandBuilder(
            IProgressiveLevelProvider progressiveProvider,
            ITransactionHistory transactions,
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IProgressiveDeviceManager progressiveDeviceManager)
        {
            _progressives = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _progressiveDeviceManager = progressiveDeviceManager ?? throw new ArgumentNullException(nameof(progressiveDeviceManager));
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

            var levels = _progressives.GetProgressiveLevels().Where(l => l.ProgressiveId == device.ProgressiveId && (_progressiveDeviceManager.VertexDeviceIds.TryGetValue(l.DeviceId, out int value) ? value : l.DeviceId) == device.Id).ToList();
            var lvl = levels.FirstOrDefault();

            command.configurationId = device.ConfigurationId;
            command.minLogEntries = _transactions.GetMaxTransactions<JackpotTransaction>();
            command.noProgInfo = device.NoProgInfo;
            command.noResponseTimer = (int)device.NoResponseTimer.TotalMilliseconds;
            command.progId = levels.First().ProgressiveId;
            command.requiredForPlay = device.RequiredForPlay;
            command.restartStatus = device.RestartStatus;
            command.timeToLive = device.TimeToLive;
            command.useDefaultConfig = device.UseDefaultConfig;
            command.configComplete = device.ConfigComplete;
            command.configDateTime = device.ConfigDateTime;

            var levelProfiles = new List<levelProfile>();
            foreach(var level in levels)
            {
                IViewableLinkedProgressiveLevel linkedLevel = null;

                if (!string.IsNullOrEmpty(level?.AssignedProgressiveId?.AssignedProgressiveKey))
                {
                    _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(level?.AssignedProgressiveId?.AssignedProgressiveKey, out linkedLevel);
                }

                var levelId = linkedLevel?.ProtocolLevelId ?? level.LevelId;

                if (levelProfiles.Any(p => p.levelId == levelId))
                    continue;

                var profile = new levelProfile();
                profile.levelId = levelId;
                var levelType = level.FundingType.ToString().ToLower();
                profile.levelType = $"{Constants.ManufacturerPrefix}_{levelType}";
                profile.incrementRate = level.IncrementRate;
                profile.maxValue = level.MaximumValue;
                profile.resetValue = level.ResetValue;

                var gameLevelConfigs = new List<gameLevelConfig>();
                var game = _gameProvider.GetGame(level.GameId);
                foreach (var denom in game.Denominations)
                {
                    if (!denom.Active)
                        continue;

                    var variation = int.Parse(game.VariationId);
                    var config = new gameLevelConfig();
                    config.denomId = denom.Id;
                    config.gamePlayId = level.GameId;
                    config.numberOfCredits = (int)denom.MinimumWagerCredits < 1 ? 1 : (int)denom.MinimumWagerCredits;
                    config.paytableId = game.PaytableId;
                    config.themeId = game.ThemeId;
                    config.turnover = level.Turnover;
                    config.linkThemeId = game.ThemeId;
                    config.winLevelCombo = level.LevelName;
                    config.winLevelIndex = level.LevelId; //May need updated in the future to match vertex levelIds
                    config.winLevelOdds = level.LevelRtp;
                    config.variation = variation;
                    gameLevelConfigs.Add(config);
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