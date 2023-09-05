namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Reflection;
    using Contracts;
    using Contracts.Events;
    using Kernel;
    using log4net;
    using Runtime;
    using Runtime.Client;
    using Vgt.Client12.Application.OperatorMenu;

    public class PresentationIdleCommandHandler : ICommandHandler<PresentationIdle>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _bus;
        private readonly IRuntime _runtime;
        private readonly IGameHistory _history;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly ICashoutController _cashoutController;

        public PresentationIdleCommandHandler(
            IRuntime runtime,
            IGameHistory history,
            ICommandHandlerFactory commandFactory,
            IOperatorMenuLauncher operatorMenu,
            ICashoutController cashoutController,
            IEventBus bus)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _cashoutController = cashoutController ?? throw new ArgumentNullException(nameof(cashoutController));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public void Handle(PresentationIdle command)
        {
            var checkBalance = new CheckBalance();

            _commandFactory.Create<CheckBalance>().Handle(checkBalance);
            _operatorMenu.EnableKey(GamingConstants.OperatorMenuDisableKey);

            _bus.Publish(new DisableCountdownTimerEvent(false));
            _bus.Publish(new AllowMoneyInEvent());

            Logger.Debug($"PresentationIdle: ForcedCashout={checkBalance.ForcedCashout}, PaperInChuteNotificationActive={_cashoutController.PaperInChuteNotificationActive}");
            if (!checkBalance.ForcedCashout && !_cashoutController.PaperIsInChute)
            {
                _runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            }
        }
    }
}