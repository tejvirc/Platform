namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Contracts.Progressives;
    using Progressives;


    /// <summary>
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
                                              .ToDictionary(
                                                  level =>(uint)level.LevelId,
                                                  level => _mysteryProgressiveProvider.CheckMysteryJackpot(level)
                                              );
        }
    }
}