namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Exceptions;
    using Gaming.Contracts;
    using Kernel;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Handles the <see cref="GameExitedNormalEvent" /> event.
    /// </summary>
    public class GameExitedNormalConsumer : Consumes<GameExitedNormalEvent>
    {
        private const int LobbyAsSelectedGame = 0;
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameExitedNormalConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="propertiesManager">The properties provider.</param>
        /// <param name="launcher">The Operator Menu launcher</param>
        public GameExitedNormalConsumer(ISasExceptionHandler exceptionHandler, IPropertiesManager propertiesManager, IOperatorMenuLauncher launcher)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _operatorMenuLauncher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        }

        /// <inheritdoc />
        public override void Consume(GameExitedNormalEvent theEvent)
        {
            var lastGameId = _propertiesManager.GetValue(SasProperties.PreviousSelectedGameId, LobbyAsSelectedGame);
            if (lastGameId != LobbyAsSelectedGame)
            {
                if (!_operatorMenuLauncher.IsShowing)
                {
                    _exceptionHandler.ReportException(new GameSelectedExceptionBuilder(LobbyAsSelectedGame));
                }

                _propertiesManager.SetProperty(SasProperties.PreviousSelectedGameId, LobbyAsSelectedGame);
            }
        }
    }
}
