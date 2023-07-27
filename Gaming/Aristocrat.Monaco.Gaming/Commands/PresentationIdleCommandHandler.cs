namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Contracts.Events;
    using Kernel;
    using Runtime;
    using Runtime.Client;
    using Vgt.Client12.Application.OperatorMenu;

    public class PresentationIdleCommandHandler : ICommandHandler<PresentationIdle>
    {
        private readonly IEventBus _bus;
        private readonly IBalanceUpdateService _balanceUpdateService;
        private readonly IRuntime _runtime;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly ICashoutController _cashoutController;
        private readonly IPlayerBank _playerBank;

        public PresentationIdleCommandHandler(
            IRuntime runtime,
            ICommandHandlerFactory commandFactory,
            IOperatorMenuLauncher operatorMenu,
            ICashoutController cashoutController,
            IPlayerBank playerBank,
            IEventBus bus,
            IBalanceUpdateService balanceUpdateService)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _cashoutController = cashoutController ?? throw new ArgumentNullException(nameof(cashoutController));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _balanceUpdateService = balanceUpdateService ?? throw new ArgumentNullException(nameof(balanceUpdateService));
        }

        public void Handle(PresentationIdle command)
        {
            var checkBalance = new CheckBalance();

            _balanceUpdateService.UpdateBalance();
            _commandFactory.Create<CheckBalance>().Handle(checkBalance);
            _operatorMenu.EnableKey(GamingConstants.OperatorMenuDisableKey);
            _playerBank.Unlock();

            _bus.Publish(new DisableCountdownTimerEvent(false));
            _bus.Publish(new AllowMoneyInEvent());

            if (!checkBalance.ForcedCashout && !_cashoutController.PaperIsInChute)
            {
                _runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            }
        }
    }
}