namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;

    /// <inheritdoc />
    public class GameAddedConsumer : Consumes<GameAddedEvent>
    {
        private readonly IGameProvider _gameProvider;
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Creates a Consumer of the event <see cref="GameAddedConsumer"/>
        /// </summary>
        /// <param name="gameProvider">An <see cref="IGameProvider"/> instance</param>
        /// <param name="exceptionHandler">An <see cref="ISasExceptionHandler"/> instance</param>
        public GameAddedConsumer(IGameProvider gameProvider, ISasExceptionHandler exceptionHandler)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(GameAddedEvent theEvent)
        {
            _gameProvider.EnableGame(theEvent.GameId, GameStatus.DisabledByBackend);
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.OperatorChangedOptions));
        }
    }
}