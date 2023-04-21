namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Contracts.Progressives;
    using Progressives;


    /// <summary>
    ///     Returns a List of progressive level Id's for Mystery levels that have been won.
    ///     Command handler for the <see cref="CheckMysteryJackpot" /> command.
    /// </summary>
    public class CheckMysteryJackpotCommandHandler : ICommandHandler<CheckMysteryJackpot>
    {
        private readonly IProgressiveGameProvider _progressiveGame;
        private readonly IMysteryProgressiveProvider _mysteryProgressiveProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CheckMysteryJackpotCommandHandler" /> class.
        /// </summary>
        public CheckMysteryJackpotCommandHandler(
            IProgressiveGameProvider progressiveGame,
            IMysteryProgressiveProvider mysteryProgressiveProvider
        )
        {
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
            _mysteryProgressiveProvider = mysteryProgressiveProvider ??
                                          throw new ArgumentNullException(nameof(mysteryProgressiveProvider));
        }

        /// <inheritdoc />
        public void Handle(CheckMysteryJackpot command)
        {
            command.Results = _progressiveGame.GetActiveProgressiveLevels()
                                              .Where(progressive => progressive.TriggerControl == TriggerType.Mystery)
                                              .Where(level => _mysteryProgressiveProvider.CheckMysteryJackpot(level))
                                              .Select(level => (uint)level.LevelId)
                                              .ToList();
        }
    }
}