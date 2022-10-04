namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts.Diagnostics;
    using Contracts;
    using Contracts.Progressives;
    using Progressives;
    using Runtime;

    /// <summary>
    ///     Command handler for the <see cref="TriggerJackpot" /> command.
    /// </summary>
    public class TriggerJackpotCommandHandler : ICommandHandler<TriggerJackpot>
    {
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IProgressiveGameProvider _progressiveGame;
        private readonly IRuntime _runtime;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TriggerJackpotCommandHandler" /> class.
        /// </summary>
        public TriggerJackpotCommandHandler(
            IProgressiveGameProvider progressiveGame,
            IGameDiagnostics gameDiagnostics,
            IRuntime runtime)
        {
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        /// <inheritdoc />
        public void Handle(TriggerJackpot command)
        {
            if (_gameDiagnostics.IsActive)
            {
                var jackpots = new List<JackpotInfo>();

                if (_gameDiagnostics.Context is ICombinationTestContext)
                {
                    var gameProgressiveLevels = _progressiveGame.GetProgressiveLevel(command.PoolName, false);
                    foreach (var levelId in command.LevelIds)
                    {
                        var level = gameProgressiveLevels.FirstOrDefault(x => x.Key == levelId);
                        jackpots.Add(new JackpotInfo { WinAmount = level.Value, LevelId = levelId, TransactionId = levelId, });
                    }
                }
                else if (_gameDiagnostics.Context is IDiagnosticContext<IGameHistoryLog> context)
                {
                    // For replay the triggers come solely from the log
                    jackpots = context.Arguments.Jackpots.Where(t => command.TransactionIds.Contains(t.TransactionId)).ToList();
                }

                if (_gameDiagnostics.Context is ICombinationTestContext ||
                    _gameDiagnostics.Context is IDiagnosticContext<IGameHistoryLog>)
                {
                    command.Results = ToResults(jackpots);

                    // For Diagnostics, tell the runtime the jackpot win amount so it can proceed as intended.
                    Task.Run(() => NotifyGameWinAsync(command.PoolName, jackpots));
                    return;
                }
            }

            command.Results = _progressiveGame.TriggerProgressiveLevel(
                command.PoolName,
                command.LevelIds,
                command.TransactionIds,
                command.Recovering);
        }

        private void NotifyGameWinAsync(string poolName, IEnumerable<JackpotInfo> jackpots)
        {
            _runtime.JackpotWinNotification(poolName, jackpots.ToDictionary(j => j.LevelId, j => j.TransactionId));
        }

        private static IEnumerable<ProgressiveTriggerResult> ToResults(IEnumerable<JackpotInfo> jackpots)
        {
            return jackpots.Select(
                j => new ProgressiveTriggerResult { LevelId = j.LevelId, TransactionId = j.TransactionId });
        }
    }
}