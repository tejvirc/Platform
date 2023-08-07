namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;
    using Ticketing;

    /// <summary>
    ///     Handles transferring cash, promo, and restricted funds in house from the host to gaming machine
    /// </summary>
    public class AftTransferInHouseFromHostToGameMachine : IAftRequestProcessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> _errorConditions;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IBank _bank;
        private readonly IAftTransferProvider _aftProvider;
        private readonly ITicketingCoordinator _ticketingCoordinator;

        /// <summary>
        ///     Instantiates a new instance of the AftTransferInHouseFromHostToGameMachine class
        /// </summary>
        /// <param name="aftProvider">reference to the AftTransferProvider class</param>
        /// <param name="ticketingCoordinator"></param>
        /// <param name="bank">a reference to the bank</param>
        /// <param name="propertiesManager">a reference to properties manager</param>
        public AftTransferInHouseFromHostToGameMachine(
            IAftTransferProvider aftProvider,
            ITicketingCoordinator ticketingCoordinator,
            IBank bank,
            IPropertiesManager propertiesManager)
        {
            _aftProvider = aftProvider ?? throw new ArgumentNullException(nameof(aftProvider));
            _ticketingCoordinator = ticketingCoordinator ?? throw new ArgumentNullException(nameof(ticketingCoordinator));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));

            _errorConditions = InitializeErrorConditions();
        }

        /// <inheritdoc />
        public AftResponseData Process(AftResponseData data)
        {
            Logger.Debug("Aft Transfer in house to game");

            // mark request as pending. The following code will change it if there are any errors
            _aftProvider.CurrentTransfer.TransferStatus = AftTransferStatusCode.TransferPending;

            _aftProvider.CheckForErrorConditions(_errorConditions);
            if (_aftProvider.TransferFailure)
            {
                _aftProvider.AftTransferFailed();
                return _aftProvider.CurrentTransfer;
            }

            Logger.Debug("Starting Aft on as a task");
            _aftProvider.DoAftOn().FireAndForget();

            return _aftProvider.CurrentTransfer;
        }

        private Dictionary<Func<bool>, (AftTransferStatusCode code, string message)> InitializeErrorConditions()
        {
            return new Dictionary<Func<bool>, (AftTransferStatusCode code, string message)>
            {
                {   // transfer would be over the credit limit
                    () =>
                    {
                        var balanceInCents = _aftProvider.CurrentBankBalanceInCents;
                        var limit = (ulong)_bank.Limit.MillicentsToCents();
                        return _aftProvider.TransferAmount > 0 && // Always accept a zero transfer amount required by the spec
                               (balanceInCents >= limit || _aftProvider.FullTransferRequested &&
                                   _aftProvider.TransferAmount + balanceInCents > limit);
                    },
                    (AftTransferStatusCode.TransferAmountExceedsGameLimit, "transfer would be over the credit limit")
                },
                {   // if we already have restricted credits, make sure new ones are for the same pool
                    () => _aftProvider.CurrentTransfer.RestrictedAmount > 0 &&
                          _bank.QueryBalance(AccountType.NonCash) > 0 &&
                          _aftProvider.CurrentTransfer.PoolId != _ticketingCoordinator.GetData().PoolId,
                    (AftTransferStatusCode.UnableToAcceptTransferDueToExistingRestrictedAmounts, "restricted credits on different pool id")
                },
                {   // we're not configured to do any transfers on
                    () => !_propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).TransferInAllowed,
                    (AftTransferStatusCode.NotAValidTransferFunction, "configuration doesn't allow transfer on")
                },
                {   // check if we're trying to transfer more than the configured max aft transfer amount
                    // and it's not a partial transfer. Partial transfers can request over the max aft transfer amount.
                    () => _aftProvider.TransferAmount > _aftProvider.TransferLimitAmount && _aftProvider.FullTransferRequested,
                    (AftTransferStatusCode.TransferAmountExceedsGameLimit, "transfer is over the aft transfer limit")
                }
            };
        }
    }
}