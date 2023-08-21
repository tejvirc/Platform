namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Contracts;
    using Progressives;

    /// <summary>
    ///     Command handler for the <see cref="GetJackpotValues" /> command.
    /// </summary>
    public class GetJackpotValuesCommandHandler : ICommandHandler<GetJackpotValues>
    {
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IProgressiveGameProvider _progressiveGame;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetJackpotValuesCommandHandler" /> class.
        /// </summary>
        public GetJackpotValuesCommandHandler(IGameDiagnostics diagnostics, IProgressiveGameProvider progressiveGame)
        {
            _gameDiagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
        }

        /// <inheritdoc />
        public void Handle(GetJackpotValues command)
        {
            if (_gameDiagnostics.IsActive && _gameDiagnostics.Context is IDiagnosticContext<IGameHistoryLog> context)
            {
                if (context.Arguments.JackpotSnapshot != null)
                {
                    command.JackpotValues =
                        context.Arguments.JackpotSnapshot.ToDictionary(x => x.LevelId, x => x.Value);
                }
            }
            else
            {
                command.JackpotValues = _progressiveGame.GetProgressiveLevel(command.PoolName, command.Recovering);
            }
        }
    }
}