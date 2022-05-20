namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Services.CreditValidators;
    using Services.PlayerTracking;

    /// <summary>
    ///     Handles <see cref="LogoffPlayer"/> message.
    /// </summary>
    public class LogoffPlayerHandler : MessageHandler<LogoffPlayer>
    {
        private readonly ILogger<LogoffPlayerHandler> _logger;
        private readonly ICashOut _cashOutHandler;
        private readonly IGamePlayState _gamePlay;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IPlayerTracking _playerTracking;

        /// <summary>
        ///     Construct a <see cref="LogoffPlayerHandler" /> object.
        /// </summary>
        /// <param name="cashOutHandler"><see cref="ICashOut" /></param>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="gamePlay"><see cref="IGamePlayState" /></param>
        /// <param name="propertiesManager"><see cref="IPropertiesManager" /></param>
        /// <param name="playerTracking"><see cref="IPlayerTracking" /></param>
        public LogoffPlayerHandler(
            ICashOut cashOutHandler,
            ILogger<LogoffPlayerHandler> logger,
            IGamePlayState gamePlay,
            IPropertiesManager propertiesManager,
            IPlayerTracking playerTracking)
        {
            _cashOutHandler = cashOutHandler ?? throw new ArgumentNullException(nameof(cashOutHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _playerTracking = playerTracking ?? throw new ArgumentNullException(nameof(playerTracking));
        }

        /// <inheritdoc />
        public override async Task<IResponse> Handle(LogoffPlayer message)
        {
            if (_gamePlay.InGameRound)
            {
                _propertiesManager.SetProperty(MgamConstants.ForceCashoutAfterGameRoundKey, true);
                _propertiesManager.SetProperty(MgamConstants.EndPlayerSessionAfterGameRoundKey, true);
            }
            else
            {
                _playerTracking.EndPlayerSession();
                _logger.LogInfo($"Ending player session for player with {_cashOutHandler.Credits} credits");

                _cashOutHandler.CashOut();
            }

            return await Task.FromResult(Ok<LogoffPlayerResponse>());
        }
    }
}
