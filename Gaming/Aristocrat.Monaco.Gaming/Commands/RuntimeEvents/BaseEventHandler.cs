namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    using System;
    using Contracts;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public abstract class BaseEventHandler
    {
        private readonly IPlayerBank _bank;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IRuntime _runtime;
        private readonly IGameCashOutRecovery _gameCashOutRecovery;

        protected BaseEventHandler(
            IPropertiesManager properties,
            ICommandHandlerFactory commandFactory,
            IRuntime runtime,
            IPlayerBank bank,
            IGameCashOutRecovery gameCashOutRecovery)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _gameCashOutRecovery = gameCashOutRecovery ?? throw new ArgumentNullException((nameof(gameCashOutRecovery)));

            MeterFreeGames = properties.GetValue(GamingConstants.MeterFreeGamesIndependently, false);
        }

        protected bool MeterFreeGames { get; }

        protected void CheckOutcome(long result)
        {
            var checkResult = new CheckResult(result);
            _commandFactory.Create<CheckResult>().Handle(checkResult);

            var checkBalance = new CheckBalance(checkResult.AmountOut);
            _commandFactory.Create<CheckBalance>().Handle(checkBalance);

            if (!checkResult.ForcedCashout && !checkBalance.ForcedCashout)
            {
                SetAllowSubgameRound(true);
            }
        }

        protected void SetAllowSubgameRound(bool state)
        {
            _runtime.UpdateFlag(RuntimeCondition.AllowSubGameRound, state);
        }

        protected void UpdateBalance()
        {
            _runtime.UpdateBalance(_bank.Credits);
        }

        protected bool CanExitRecovery()
        {
            if (!_gameCashOutRecovery.HasPending)
            {
                return true;
            }

            SetAllowSubgameRound(false);

            return !_gameCashOutRecovery.Recover();
        }
    }
}