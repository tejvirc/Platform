namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Identification;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;
    using Common;
    using Gaming.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Localization.Properties;
    using Services.CreditValidators;
    using Services.PlayerTracking;
    using EndSession = Commands.EndSession;

    /// <summary>
    ///     Consumes <seealso cref="GameIdleEvent" /> event.
    /// </summary>
    public class GameIdleConsumer : Consumes<GameIdleEvent>
    {
        private readonly ILogger _logger;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly ICashOut _cashOutHandler;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAudio _audioService;
        private readonly ITowerLight _towerLightService;
        private readonly IPlayerTracking _playerTracking;
        private readonly IIdentificationValidator _idValidator;
        private readonly IEmployeeLogin _employeeLogin;
        private readonly ISystemDisableManager _disableManager;

        /// <summary>
        ///     Initializes a new instance of the GameIdleConsumer class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger{TCategory}"/>.</param>
        /// <param name="commandFactory"><see cref="ICommandHandlerFactory"/>.</param>
        /// <param name="cashOutHandler"><see cref="ICashOut"/>.</param>
        /// <param name="properties"><see cref="IPropertiesManager"/>.</param>
        /// <param name="audio"><see cref="IAudio"/>.</param>
        /// <param name="towerLight"><see cref="ITowerLight"/>.</param>
        /// <param name="playerTracking"><see cref="IPlayerTracking"/>.</param>
        /// <param name="idValidator"><see cref="IIdentificationValidator"/>.</param>
        /// <param name="employeeLogin"><see cref="IEmployeeLogin"/>.</param>
        /// <param name="disableManager"><see cref="ISystemDisableManager"/>.</param>
        public GameIdleConsumer(
            ILogger<GameIdleConsumer> logger,
            ICommandHandlerFactory commandFactory,
            ICashOut cashOutHandler,
            IPropertiesManager properties,
            IAudio audio,
            ITowerLight towerLight,
            IPlayerTracking playerTracking,
            IIdentificationValidator idValidator,
            IEmployeeLogin employeeLogin,
            ISystemDisableManager disableManager)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cashOutHandler = cashOutHandler ?? throw new ArgumentNullException(nameof(cashOutHandler));
            _propertiesManager = properties ?? throw new ArgumentNullException(nameof(properties));
            _audioService = audio ?? throw new ArgumentNullException(nameof(audio));
            _towerLightService = towerLight ?? throw new ArgumentNullException(nameof(towerLight));
            _playerTracking = playerTracking ?? throw new ArgumentNullException(nameof(playerTracking));
            _idValidator = idValidator ?? throw new ArgumentNullException(nameof(idValidator));
            _employeeLogin = employeeLogin ?? throw new ArgumentNullException(nameof(employeeLogin));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
        }

        /// <inheritdoc />
        public override async Task Consume(GameIdleEvent theEvent, CancellationToken cancellationToken)
        {
            try
            {
                if (_cashOutHandler.Balance == 0)
                {
                    _logger.LogInfo("SendEndSession credits 0.");
                    await _commandFactory.Execute(new EndSession());
                }
            }
            catch (ServerResponseException ex)
            {
                _logger.LogError(ex, "SendEndSession failed ServerResponseException");
            }

            if (_propertiesManager.GetValue(MgamConstants.EndPlayerSessionAfterGameRoundKey, false))
            {
                _logger.LogInfo("Ending player session");
                _playerTracking.EndPlayerSession();

                _propertiesManager.SetProperty(MgamConstants.EndPlayerSessionAfterGameRoundKey, false);
            }

            if (_propertiesManager.GetValue(MgamConstants.ForceCashoutAfterGameRoundKey, false))
            {
                if (_cashOutHandler.Balance > 0)
                {
                    _logger.LogInfo($"Forcing cashout of {_cashOutHandler.Credits} credits after game round.");
                    _cashOutHandler.CashOut();
                }

                _propertiesManager.SetProperty(MgamConstants.ForceCashoutAfterGameRoundKey, false);
            }

            if (_propertiesManager.GetValue(MgamConstants.LogoffPlayerAfterGameRoundKey, false))
            {
                _logger.LogInfo("Forcing player logoff");
                await _idValidator.LogoffPlayer();

                _propertiesManager.SetProperty(MgamConstants.LogoffPlayerAfterGameRoundKey, false);
            }

            if (_propertiesManager.GetValue(MgamConstants.PlayAlarmAfterGameRoundKey, false))
            {
                var alertVolume = _propertiesManager.GetValue(ApplicationConstants.AlertVolumeKey, MgamConstants.DefaultAlertVolume);
                _audioService.Play(SoundName.Alert, MgamConstants.DefaultAlertLoopCount, alertVolume);

                _towerLightService.SetFlashState(LightTier.Tier1, FlashState.FastFlash, TimeSpan.MaxValue);

                _propertiesManager.SetProperty(MgamConstants.PlayAlarmAfterGameRoundKey, false);
            }

            if (_propertiesManager.GetValue(MgamConstants.DisableGamePlayAfterGameRoundKey, false))
            {
                _logger.LogInfo("Forcing disable game play");

                _disableManager.Disable(
                    MgamConstants.GamePlayDisabledKey,
                    SystemDisablePriority.Normal,
                    () =>
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByHost));

                _propertiesManager.SetProperty(MgamConstants.DisableGamePlayAfterGameRoundKey, false);
            }

            if (_propertiesManager.GetValue(MgamConstants.EnterDropModeAfterGameRoundKey, false))
            {
                _logger.LogInfo("Entering Drop Mode after game round");

                // Drop Mode works like a virtual employee login.
                _employeeLogin.Login(ResourceKeys.DropMode);

                _audioService.SetSystemMuted(true);

                _propertiesManager.SetProperty(MgamConstants.EnterDropModeAfterGameRoundKey, false);
            }
        }
    }
}