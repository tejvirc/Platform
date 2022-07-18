namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Accounting.Contracts;
    using AftTransferProvider;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Handles transferring cash, promo, and restricted funds from gaming machine to host
    /// </summary>
    public class AftTransferInHouseFromGameMachineToHost : IAftRequestProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAftHostCashOutProvider _aftHostCashOutProvider;
        private readonly IPropertiesManager _propertiesManager;
        private readonly Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> _errorConditions;
        private readonly IAftTransferProvider _aftProvider;

        /// <summary>
        ///     Instantiates a new instance of the AftTransferInHouseFromGameMachineToHost class
        /// </summary>
        /// <param name="aftProvider">reference to the AftTransferProvider class</param>
        /// <param name="aftHostCashOutProvider">The host cashout provider</param>
        /// <param name="propertiesManager">A reference to the PropertiesManager</param>
        public AftTransferInHouseFromGameMachineToHost(
            IAftTransferProvider aftProvider,
            IAftHostCashOutProvider aftHostCashOutProvider,
            IPropertiesManager propertiesManager)
        {
            _aftProvider = aftProvider ?? throw new ArgumentNullException(nameof(aftProvider));
            _aftHostCashOutProvider = aftHostCashOutProvider ?? throw new ArgumentNullException(nameof(aftHostCashOutProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _errorConditions = InitializeErrorConditions();
        }

        /// <inheritdoc />
        public AftResponseData Process(AftResponseData data)
        {
            Logger.Debug("Aft Transfer in house from game to host");

            // mark request as pending. The following code will change it if there are any errors
            _aftProvider.CurrentTransfer.TransferStatus = AftTransferStatusCode.TransferPending;

            _aftProvider.CheckForErrorConditions(_errorConditions);
            if (_aftProvider.TransferFailure)
            {
                _aftProvider.AftTransferFailed();
                return _aftProvider.CurrentTransfer;
            }

            Logger.Debug("Starting Aft off to host as a task");
            _aftProvider.DoAftOff().FireAndForget();

            return _aftProvider.CurrentTransfer;
        }

        /// <summary>
        ///     create a list of error conditions to check.
        /// </summary>
        private Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> InitializeErrorConditions()
        {
            return new Dictionary<Func<bool>, (AftTransferStatusCode code, string message)>
            {
                {   // no money to transfer off
                    () => _aftProvider.FullTransferRequested && _aftProvider.TransferAmount > 0 && _aftProvider.CurrentBankBalanceInCents == 0,
                    (AftTransferStatusCode.NotAValidTransferAmountOrExpirationDate, "no money to transfer off")
                },
                {   // we're not configured to do any transfers off
                    () => !_propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).TransferOutAllowed,
                    (AftTransferStatusCode.NotAValidTransferFunction, "configuration doesn't allow transfer off")
                },
                {   // check if we're trying to transfer more than the configured max aft transfer amount
                    // and it's not a partial transfer. Partial transfers can request over the max aft transfer amount.
                    () => _aftProvider.TransferAmount > _aftProvider.TransferLimitAmount && _aftProvider.FullTransferRequested,
                    (AftTransferStatusCode.TransferAmountExceedsGameLimit, "transfer is over the aft transfer limit")
                },
                {   // not enough cashable money to do a full transfer off
                    () => _aftProvider.FullTransferRequested &&
                          _aftProvider.CurrentTransfer.CashableAmount > _aftProvider.CurrentBankBalanceInCentsForAccount(AccountType.Cashable),
                    (AftTransferStatusCode.NotAValidTransferFunction, "not enough cashable money to do a full transfer off")
                },
                {   // not enough restricted money to do a full transfer off
                    () => _aftProvider.FullTransferRequested &&
                          _aftProvider.CurrentTransfer.RestrictedAmount > _aftProvider.CurrentBankBalanceInCentsForAccount(AccountType.NonCash),
                    (AftTransferStatusCode.NotAValidTransferFunction, "not enough restricted money to do a full transfer off")
                },
                {   // not enough promo money to do a full transfer off
                    () => _aftProvider.FullTransferRequested &&
                          _aftProvider.CurrentTransfer.NonRestrictedAmount > _aftProvider.CurrentBankBalanceInCentsForAccount(AccountType.Promo),
                    (AftTransferStatusCode.NotAValidTransferFunction, "not enough promo money to do a full transfer off")
                },
                {
                    () => _aftHostCashOutProvider.CashOutWinPending,
                    (AftTransferStatusCode.GamingMachineUnableToPerformTransfer, "currently pending a host cashout to win we need to cashout using Cashout Win from host")
                }
            };
        }
    }
}