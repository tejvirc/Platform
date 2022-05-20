namespace Aristocrat.Monaco.Gaming
{
    using System;
    using Accounting.Contracts;
    using Contracts;
    using Kernel;

    /// <summary>
    ///     Provides a method to indicate if we can cash out during
    ///     a lockup state
    /// </summary>
    public class CashableLockupProvider : ICashableLockupProvider
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IFundsTransferDisable _fundsTransferDisable;
        private readonly ITransactionCoordinator _transactionCoordinator;

        /// <summary>
        ///     Constructs an instance of the CashableLockupProvider
        /// </summary>
        /// <param name="propertiesManager">A reference to the PropertiesManager class</param>
        /// <param name="fundsTransferDisable">A reference to the FundsTransferDisable class</param>
        /// <param name="transactionCoordinator">A reference to the TransactionCoordinator class</param>
        public CashableLockupProvider(
            IPropertiesManager propertiesManager,
            IFundsTransferDisable fundsTransferDisable,
            ITransactionCoordinator transactionCoordinator)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _fundsTransferDisable =
                fundsTransferDisable ?? throw new ArgumentNullException(nameof(fundsTransferDisable));
            _transactionCoordinator =
                transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
        }

        /// <inheritdoc />
        public bool CanCashoutInLockup(bool isLockupMessageVisible, bool cashOutEnabled, Action cashoutMethod)
        {
            // Do not allow a cashout transaction if there is already a transaction in progress.
            if (_transactionCoordinator.IsTransactionActive)
            {
                return false;
            }

            // in a disabled state we can do the following based on the LockupBehavior setting
            // in the Gaming.config.xml file:
            // 1. Do nothing except show the tilt message - this is the default behavior
            // 2. Conditionally show a cashout button depending on the tilt
            // 3. Force a cashout - this will be a future story
            var action = (CashableLockupStrategy)_propertiesManager.GetProperty(
                GamingConstants.LockupBehavior,
                CashableLockupStrategy.NotAllowed);
            switch (action)
            {
                case CashableLockupStrategy.Allowed:
                    return isLockupMessageVisible && !_fundsTransferDisable.TransferOffDisabled &&
                           cashOutEnabled;
                case CashableLockupStrategy.ForceCashout:
                    return false;
                default:
                    return false;
            }
        }
    }
}