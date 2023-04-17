namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="RequestCashoutCommandHandler" /> class.
        /// </summary>
        /// <param name="runtime">An <see cref="IRuntimeFlagHandler" /> instance.</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        /// <param name="playerBank">An <see cref="IPlayerBank" /> instance.</param>
        /// <param name="cashoutController"></param>
        public RequestCashoutCommandHandler(
            IRuntimeFlagHandler runtime,
            IPropertiesManager properties,
            IPlayerBank playerBank,
            ICashoutController cashoutController)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _cashoutController = cashoutController ?? throw new ArgumentNullException(nameof(cashoutController));
        }

        /// <inheritdoc />
        public void Handle(RequestCashout command)
        {
            if (_playerBank.Balance == 0)
            {
                return;
            }

            _runtime.SetCashingOut(true);
            _cashoutController.GameRequestedCashout();
        }
    }
}