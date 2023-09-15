namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.OperatorMenu;
    using Contracts;
    using Contracts.Events.OperatorMenu;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Implements <see cref="IGamingAccessEvaluation" /> interface
    /// </summary>
    public class AccessEvaluationService : IGamingAccessEvaluation, IAccessEvaluatorSource
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IGameHistory _gameHistory;
        private readonly IGamePlayState _gamePlayState;
        private readonly IMeterManager _meters;
        private readonly IOperatorMenuAccess _operatorMenuAccess;
        private readonly IPropertiesManager _properties;

        private bool _zeroGamesPlayed = true;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccessEvaluationService" /> class.
        /// </summary>
        /// <param name="eventBus"></param>
        /// <param name="properties"></param>
        /// <param name="meters"></param>
        /// <param name="gamePlayState"></param>
        /// <param name="gameHistory"></param>
        public AccessEvaluationService(
            IEventBus eventBus,
            IPropertiesManager properties,
            IMeterManager meters,
            IGamePlayState gamePlayState,
            IGameHistory gameHistory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _operatorMenuAccess = ServiceManager.GetInstance().GetService<IOperatorMenuAccess>();
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGamingAccessEvaluation) };

        /// <inheritdoc />
        public void Initialize()
        {
            _operatorMenuAccess.RegisterAccessRuleEvaluator(
                this,
                OperatorMenuAccessRestriction.InGameRound,
                EvaluateInGameRound);
            _operatorMenuAccess.RegisterAccessRuleEvaluator(
                this,
                OperatorMenuAccessRestriction.GameLoaded,
                EvaluateGameLoaded);
            _operatorMenuAccess.RegisterAccessRuleEvaluator(
                this,
                OperatorMenuAccessRestriction.GamesPlayed,
                EvaluateZeroGamesPlayed);
            _operatorMenuAccess.RegisterAccessRuleEvaluator(
                this,
                OperatorMenuAccessRestriction.InitialGameConfigurationComplete,
                EvaluateInitialGameConfigurationComplete);

            _eventBus.Subscribe<GameIdleEvent>(this, HandleEvent);
            _eventBus.Subscribe<GamePlayInitiatedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameConnectedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameProcessExitedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameConfigurationSaveCompleteEvent>(this, HandleEvent);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private bool EvaluateInGameRound()
        {
            var access = !_gamePlayState.InGameRound && !_gameHistory.IsRecoveryNeeded;
            Logger.Debug($"In Game Round: {access}");
            return access;
        }

        private bool EvaluateGameLoaded()
        {
            var access = !_properties.GetValue(GamingConstants.IsGameRunning, false);
            Logger.Debug($"Game Loaded: {access}");
            return access;
        }

        private bool EvaluateZeroGamesPlayed()
        {
            var access = false;

            if (_zeroGamesPlayed)
            {
                if (_meters.IsMeterProvided(GamingMeters.PlayedCount))
                {
                    var playedCountMeter = _meters.GetMeter(GamingMeters.PlayedCount);
                    access = (playedCountMeter?.Lifetime ?? 0) == 0;
                    _zeroGamesPlayed = access;
                }
            }

            Logger.Debug($"Zero Games Played: {access}");

            return access;
        }

        private bool EvaluateInitialGameConfigurationComplete()
        {
            var access = (bool)_properties.GetProperty(
                ApplicationConstants.GameConfigurationInitialConfigComplete,
                false);
            Logger.Debug($"Initial Game Configuration Complete : {access}");
            return access;
        }

        private void HandleEvent(GameConfigurationSaveCompleteEvent obj)
        {
            _properties.SetProperty(ApplicationConstants.GameConfigurationInitialConfigComplete, true);
            _operatorMenuAccess.UpdateAccessForRestriction(OperatorMenuAccessRestriction.InitialGameConfigurationComplete);
            _operatorMenuAccess.UpdateAccessForRestriction(OperatorMenuAccessRestriction.InitialGameConfigNotCompleteOrEKeyVerified);
        }

        private void HandleEvent(GameIdleEvent evt)
        {
            OnGamePlayStateChanged();
        }

        private void HandleEvent(GamePlayInitiatedEvent evt)
        {
            if (_zeroGamesPlayed)
            {
                _zeroGamesPlayed = false;
                _operatorMenuAccess.UpdateAccessForRestriction(OperatorMenuAccessRestriction.GamesPlayed);
            }

            OnGamePlayStateChanged();
        }

        private void HandleEvent(GameConnectedEvent evt)
        {
            OnRuntimeStateChanged();
        }

        private void HandleEvent(GameProcessExitedEvent evt)
        {
            OnRuntimeStateChanged();
        }

        private void OnGamePlayStateChanged()
        {
            _operatorMenuAccess.UpdateAccessForRestriction(OperatorMenuAccessRestriction.InGameRound);
        }

        private void OnRuntimeStateChanged()
        {
            _operatorMenuAccess.UpdateAccessForRestriction(OperatorMenuAccessRestriction.GameLoaded);
        }
    }
}