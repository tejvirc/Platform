namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Contracts.Bonus;

    public class PayBonusCommandHandler : ICommandHandler<PayBonus>
    {
        private readonly IBonusHandler _bonusHandler;
        private readonly IPlayerBank _bank;

        public PayBonusCommandHandler(IBonusHandler bonusHandler, IPlayerBank bank)
        {
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        public void Handle(PayBonus command)
        {
            command.PendingPayment = _bonusHandler.Commit(_bank.TransactionId);
        }
    }
}