namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Command handler for the <see cref="GamePlayDisabled" /> command.
    /// </summary>
    public class GamePlayDisabledCommandHandler : ICommandHandler<GamePlayDisabled>
    {
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly IResponsibleGaming _responsibleGaming;
        private readonly IHandpayRuntimeFlagsHelper _helper;

        public GamePlayDisabledCommandHandler(
            IResponsibleGaming responsibleGaming,
            IOperatorMenuLauncher operatorMenu,
            IHandpayRuntimeFlagsHelper helper)
        {
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _responsibleGaming = responsibleGaming ?? throw new ArgumentNullException(nameof(responsibleGaming));
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public void Handle(GamePlayDisabled command)
        {
            // The game needs to be paused during a lockup if it's in a game round. It will resume when the lockup is cleared.
            // Need to allow rendering of overlay animations if supported.
            // Other game rendering should be disabled.
            _helper.SetHandpayRuntimeLockupFlags();

            _operatorMenu.EnableKey(GamingConstants.OperatorMenuDisableKey);
            _responsibleGaming.OnGamePlayDisabled();
        }
    }
}
