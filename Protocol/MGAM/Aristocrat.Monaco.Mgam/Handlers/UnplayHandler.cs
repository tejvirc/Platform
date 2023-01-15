namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Identification;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Common;
    using Common.Events;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;
    using Services.CreditValidators;

    /// <summary>
    ///     Handles <see cref="Unplay" /> message.
    /// </summary>
    public class UnplayHandler : MessageHandler<Unplay>
    {
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _disableManager;
        private readonly ICashOut _cashOutHandler;
        private readonly IGamePlayState _gamePlay;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IIdentificationValidator _idValidator;

        /// <summary>
        ///     Construct a <see cref="UnplayHandler" /> object.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="disableManager"><see cref="ISystemDisableManager"/>.</param>
        /// <param name="cashOutHandler"><see cref="ICashOut"/>.</param>
        /// <param name="gamePlay"><see cref="IGamePlayState"/>.</param>
        /// <param name="propertiesManager"><see cref="IPropertiesManager"/>.</param>
        /// <param name="idValidator"><see cref="IIdentificationValidator"/>.</param>
        public UnplayHandler(
            ILogger<UnplayHandler> logger,
            IEventBus eventBus,
            ISystemDisableManager disableManager,
            ICashOut cashOutHandler,
            IGamePlayState gamePlay,
            IPropertiesManager propertiesManager,
            IIdentificationValidator idValidator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _cashOutHandler = cashOutHandler ?? throw new ArgumentNullException(nameof(cashOutHandler));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _idValidator = idValidator ?? throw new ArgumentNullException(nameof(idValidator));
        }

        ///<inheritdoc />
        public override Task<IResponse> Handle(Unplay message)
        {
            if (_gamePlay.InGameRound)
            {
                _propertiesManager.SetProperty(MgamConstants.ForceCashoutAfterGameRoundKey, true);
                _propertiesManager.SetProperty(MgamConstants.LogoffPlayerAfterGameRoundKey, true);
                _propertiesManager.SetProperty(MgamConstants.DisableGamePlayAfterGameRoundKey, true);
            }
            else
            {
                _logger.LogInfo($"Forcing logoff for player with credits {_cashOutHandler.Credits}");
                _cashOutHandler.CashOut();
                _idValidator.LogoffPlayer();

                _disableManager.Disable(
                    MgamConstants.GamePlayDisabledKey,
                    SystemDisablePriority.Normal,
                    ResourceKeys.DisabledByHost,
                    CultureProviderType.Player);
            }

            _eventBus.Publish(new UnplayEvent());

            return Task.FromResult(Ok<UnplayResponse>());
        }
    }
}