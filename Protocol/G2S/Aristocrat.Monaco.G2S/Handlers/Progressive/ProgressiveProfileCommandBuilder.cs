namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Progressives;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class ProgressiveProfileCommandBuilder : ICommandBuilder<IProgressiveDevice, progressiveProfile>
    {
        private readonly ITransactionHistory _transactions;
        private readonly IProgressiveLevelProvider _progressives;

        public ProgressiveProfileCommandBuilder(IProgressiveLevelProvider progressiveProvider, ITransactionHistory transactions)
        {
            _progressives = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
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

            command.configurationId = device.ConfigurationId;
            command.restartStatus = device.RestartStatus;
            command.minLogEntries = _transactions.GetMaxTransactions<JackpotTransaction>();
            command.timeToLive = device.TimeToLive;
            command.noProgInfo = device.NoProgressiveInfo;
            command.configComplete = device.ConfigComplete;
            command.configDateTime = device.ConfigDateTime;
            command.useDefaultConfig = device.UseDefaultConfig;
            command.requiredForPlay = device.RequiredForPlay;

            var levels = _progressives.GetProgressiveLevels().Where(p => p.DeviceId == device.Id);

            //command.progId = progressive.ProgId;
            command.levelProfile = (from level in levels
                select new levelProfile
                {
                    levelId = level.LevelId,
                    levelType = $"{Constants.ManufacturerPrefix}_{level.LevelType}",
                    incrementRate = level.IncrementRate,
                    maxValue = level.MaximumValue,
                    resetValue = level.ResetValue,
                    /*
                    gameLevelConfig = level.Triggers == null
                        ? new gameLevelConfig[0]
                        : (from trigger in level.Triggers
                            select new gameLevelConfig
                            {
                                denomId = trigger.DenomId,
                                numberOfCredits = trigger.NumberOfCredits < 1 ? 1 : trigger.NumberOfCredits,
                                gamePlayId = trigger.GameId,
                                winLevelIndex = trigger.WinLevelIndex,
                                winLevelCombo = _gameProvider.GetGame(trigger.GameId)?.WinLevels
                                    .FirstOrDefault(a => a.WinLevelIndex == trigger.WinLevelIndex)?.WinLevelCombo,
                                winLevelOdds = trigger.WinLevelOdds,
                                themeId = _gameProvider.GetGame(trigger.GameId)?.ThemeId,
                                paytableId = _gameProvider.GetGame(trigger.GameId)?.PaytableId
                            }).ToArray()
                    */
                }).ToArray();

            if (command.levelProfile?.Length == 0)
            {
                command.levelProfile = new levelProfile[1];
                command.levelProfile[0] = new levelProfile { levelId = 0, gameLevelConfig = new gameLevelConfig[0] };
            }

            await Task.CompletedTask;
        }
    }
}