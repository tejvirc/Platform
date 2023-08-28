﻿namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.HandCount;
    using Contracts;
    using Kernel;

    /// <summary>
    ///     Command handler for the <see cref="RequestCashout" /> command.
    /// </summary>
    public class RequestCashoutCommandHandler : ICommandHandler<RequestCashout>
    {
        private readonly IPropertiesManager _properties;
        private readonly IRuntimeFlagHandler _runtime;
        private readonly IPlayerBank _playerBank;
        private readonly ICashoutController _cashoutController;
        private readonly IHandCountService _handCountService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RequestCashoutCommandHandler" /> class.
        /// </summary>
        /// <param name="runtime">An <see cref="IRuntimeFlagHandler" /> instance.</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        /// <param name="playerBank">An <see cref="IPlayerBank" /> instance.</param>
        /// <param name="cashoutController"></param>
        /// <param name="handCountService">An <see cref="IHandCountService"/> instance</param>
        public RequestCashoutCommandHandler(
            IRuntimeFlagHandler runtime,
            IPropertiesManager properties,
            IPlayerBank playerBank,
            ICashoutController cashoutController,
            IHandCountService handCountService)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _cashoutController = cashoutController ?? throw new ArgumentNullException(nameof(cashoutController));
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
        }

        private bool AllowZeroCreditCashout => _properties.GetValue(GamingConstants.AllowZeroCreditCashout, false);

        /// <inheritdoc />
        public void Handle(RequestCashout command)
        {
            if (_playerBank.Balance == 0)
            {
                if (AllowZeroCreditCashout)
                {
                    // No need to set RuntimeCondition.CashingOut flag, just publish the event
                    _cashoutController.GameRequestedCashout();
                }
                return;
            }

            if (_handCountService.HandCountServiceEnabled && !CanCashoutFromHandcount())
            {
                return;
            }

            _runtime.SetCashingOut(true);
            _cashoutController.GameRequestedCashout();
        }

        private bool CanCashoutFromHandcount()
        {
            var minimumRequiredCredits = _properties.GetValue(
                AccountingConstants.HandCountMinimumRequiredCredits,
                AccountingConstants.HandCountDefaultRequiredCredits);

            return !(_playerBank.Balance < minimumRequiredCredits || _handCountService.HandCount == 0);
        }
    }
}