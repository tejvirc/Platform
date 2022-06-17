namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Exceptions;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles the <see cref="GameSelectionScreenEvent" /> event.
    /// </summary>
    public class GameSelectionScreenConsumer : Consumes<GameSelectionScreenEvent>
    {
        private const int SelectionScreenAsSelectedGame = 0;
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameSelectionScreenConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public GameSelectionScreenConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(GameSelectionScreenEvent theEvent)
        {
            // Note that when exiting the selection screen we don't need to do anything, as the
            // game or denomination change will be caught and handled elsewhere.
            if (theEvent.IsEntering)
            {
                _exceptionHandler.ReportException(new GameSelectedExceptionBuilder(SelectionScreenAsSelectedGame));
            }
        }
    }
}