namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Kernel;
    using Runtime;
    using Runtime.Client;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Command handler for the <see cref="GamePlayEnabled" /> command.
    /// </summary>
    public class GamePlayEnabledCommandHandler : ICommandHandler<GamePlayEnabled>
    {
        private readonly IResponsibleGaming _responsibleGaming;
        private readonly IRuntime _runtime;
        private readonly IPropertiesManager _properties;
        private readonly IGamePlayState _gamePlayState;
        private readonly IOperatorMenuLauncher _operatorMenu;

        public GamePlayEnabledCommandHandler(
            IResponsibleGaming responsibleGaming,
            IRuntime runtime,
            IPropertiesManager properties,
            IGamePlayState gamePlayState,
            IOperatorMenuLauncher operatorMenu)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _responsibleGaming = responsibleGaming ?? throw new ArgumentNullException(nameof(responsibleGaming));
        }

        public void Handle(GamePlayEnabled command)
        {
            // Operator menu can only be entered in game round when there is a lockup.
            if (_gamePlayState.InGameRound && !_gamePlayState.InPresentationIdle &&
                _properties.GetValue(GamingConstants.OperatorMenuDisableDuringGame, false))
            {
                _operatorMenu.DisableKey(GamingConstants.OperatorMenuDisableKey);
            }

            _runtime.UpdateFlag(RuntimeCondition.InOverlayLockup, false);
            _runtime.UpdateFlag(RuntimeCondition.InLockup, false);

            _responsibleGaming.OnGamePlayEnabled();

            if (_gamePlayState.Idle && _runtime.Connected)
            {
                _runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            }
        }
    }
}
