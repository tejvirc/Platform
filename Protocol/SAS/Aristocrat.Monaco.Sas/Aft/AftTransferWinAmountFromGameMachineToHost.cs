namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AftTransferProvider;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Handles transferring win amounts from the game machine to the host
    /// </summary>
    public class AftTransferWinAmountFromGameMachineToHost : IAftRequestProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _propertiesManager;
        private readonly IAftTransferProvider _aftProvider;
        private readonly IAftHostCashOutProvider _aftHostCashOutProvider;
        private readonly Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> _errorConditions;

        /// <summary>
        ///     Instantiates a new instance of the AftTransferWinAmountFromGameMachineToHost class
        /// </summary>
        /// <param name="aftProvider">reference to the aft transfer provider</param>
        /// <param name="aftHostCashOutProvider">the host cashout provider</param>
        /// <param name="propertiesManager">a reference to properties manager</param>
        public AftTransferWinAmountFromGameMachineToHost(
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
            // mark request as pending. The following code will change it if there are any errors
            _aftProvider.CurrentTransfer.TransferStatus = AftTransferStatusCode.TransferPending;

            _aftProvider.CheckForErrorConditions(_errorConditions);
            if (_aftProvider.TransferFailure)
            {
                _aftProvider.AftTransferFailed();
            }
            else
            {
                Logger.Debug("Starting Aft win amount off as a task");
                _aftProvider.DoAftOff().FireAndForget();
            }

            return _aftProvider.CurrentTransfer;
        }

        /// <summary>
        ///     create a list of error conditions to check.
        /// </summary>
        private Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> InitializeErrorConditions()
        {
            return new Dictionary<Func<bool>, (AftTransferStatusCode code, string message)>
            {
                {
                    // check if we're allowed to do game win to host transfers
                    () => !_propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).WinTransferAllowed,
                    (AftTransferStatusCode.NotAValidTransferFunction,
                        "configuration doesn't support win amount to host transfers")
                },
                {
                    () => !_aftHostCashOutProvider.CanCashOut || !_aftHostCashOutProvider.CashOutWinPending,
                    (AftTransferStatusCode.NoWonCreditsAvailableForCashOut,
                        "no win credits available for host cash out")
                },
                {
                    // check if we're trying to transfer more than the configured max aft transfer amount
                    // and it's not a partial transfer. Partial transfers can request over the max aft transfer amount.
                    () => _aftProvider.TransferAmount > _aftProvider.TransferLimitAmount &&
                          _aftProvider.FullTransferRequested,
                    (AftTransferStatusCode.TransferAmountExceedsGameLimit, "transfer is over the aft transfer limit")
                },
                {
                    () => _aftProvider.FullTransferRequested &&
                          (long)_aftProvider.CurrentTransfer.CashableAmount >
                          _aftHostCashOutProvider.CashOutTransaction?.CashableAmount.MillicentsToCents(),
                    (AftTransferStatusCode.NotAValidTransferFunction,
                        "not enough cashable money to do a full transfer off")
                },
                {
                    // not enough restricted money to do a full transfer off
                    () => _aftProvider.FullTransferRequested &&
                          (long)_aftProvider.CurrentTransfer.RestrictedAmount >
                          _aftHostCashOutProvider.CashOutTransaction?.NonCashAmount.MillicentsToCents(),
                    (AftTransferStatusCode.NotAValidTransferFunction,
                        "not enough restricted money to do a full transfer off")
                },
                {
                    // not enough promo money to do a full transfer off
                    () => _aftProvider.FullTransferRequested &&
                          (long)_aftProvider.CurrentTransfer.NonRestrictedAmount >
                          _aftHostCashOutProvider.CashOutTransaction?.PromoAmount.MillicentsToCents(),
                    (AftTransferStatusCode.NotAValidTransferFunction,
                        "not enough promo money to do a full transfer off")
                }
            };
        }
    }
}