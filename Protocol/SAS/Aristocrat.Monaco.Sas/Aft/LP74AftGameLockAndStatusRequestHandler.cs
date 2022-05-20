namespace Aristocrat.Monaco.Sas.Aft
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using AftTransferProvider;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Storage.Models;
    using Storage.Repository;
    using Ticketing;

    /// <summary>
    ///     Handles game lock and status
    /// </summary>
    public class
        LP74AftGameLockAndStatusRequestHandler : ISasLongPollHandler<AftGameLockAndStatusResponseData,
            AftGameLockAndStatusData>
    {
        private readonly IAftLockHandler _aftLockHandler;
        private readonly IBank _bank;
        private readonly IHostCashOutProvider _hostCashOutProvider;
        private readonly ITicketingCoordinator _ticketingCoordinator;
        private readonly IStorageDataProvider<AftTransferOptions> _aftOptionsProvider;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAftTransferAssociations _aftTransferAssociations;

        /// <summary>
        ///     Creates a new instance of the LP74AftGameLockAndStatusRequestHandler
        /// </summary>
        /// <param name="aftLockHandler">a reference to the <see cref="IAftLockHandler" /></param>
        /// <param name="bank">a reference to the <see cref="IBank" /></param>
        /// <param name="hostCashOutProvider">a reference to the <see cref="IHostCashOutProvider" /></param>
        /// <param name="ticketingCoordinator">a reference to the <see cref="ITicketingCoordinator" /></param>
        /// <param name="aftOptionsProvider"></param>
        /// <param name="propertiesManager">a reference to the <see cref="IPropertiesManager" /></param>
        /// <param name="aftTransferAssociations">a reference to the <see cref="IAftTransferAssociations" /></param>
        public LP74AftGameLockAndStatusRequestHandler(
            IAftLockHandler aftLockHandler,
            IBank bank,
            IHostCashOutProvider hostCashOutProvider,
            ITicketingCoordinator ticketingCoordinator,
            IStorageDataProvider<AftTransferOptions> aftOptionsProvider,
            IPropertiesManager propertiesManager,
            IAftTransferAssociations aftTransferAssociations)
        {
            _aftLockHandler = aftLockHandler ?? throw new ArgumentNullException(nameof(aftLockHandler));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _hostCashOutProvider = hostCashOutProvider ?? throw new ArgumentNullException(nameof(hostCashOutProvider));
            _ticketingCoordinator =
                ticketingCoordinator ?? throw new ArgumentNullException(nameof(ticketingCoordinator));
            _aftOptionsProvider = aftOptionsProvider ?? throw new ArgumentNullException(nameof(aftOptionsProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _aftTransferAssociations = aftTransferAssociations ??
                                       throw new ArgumentNullException(nameof(aftTransferAssociations));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.AftGameLockAndStatusRequest };

        /// <inheritdoc />
        public AftGameLockAndStatusResponseData Handle(AftGameLockAndStatusData data)
        {
            DoLockAction(data);
            _hostCashOutProvider.ResetCashOutExceptionTimer();
            return GenerateResponse();
        }

        private void DoLockAction(AftGameLockAndStatusData data)
        {
            switch (data.LockCode)
            {
                case AftLockCode.RequestLock when _aftTransferAssociations.TransferConditionsMet(data):
                    _aftLockHandler.AftLockTransferConditions = data.TransferConditions;
                    _aftLockHandler.AftLock(true, (uint)data.LockTimeout);
                    break;
                case AftLockCode.CancelLockOrPendingLockRequest:
                    _aftLockHandler.AftLock(false, 0);
                    break;
            }
        }

        private AftGameLockAndStatusResponseData GenerateResponse()
        {
            var response = new AftGameLockAndStatusResponseData
            {
                GameLockStatus = _aftLockHandler.LockStatus,
                AvailableTransfers = _aftTransferAssociations.GetAvailableTransfers(),
                HostCashoutStatus = GetHostCashoutStatus(),
                AftStatus = _aftTransferAssociations.GetAftStatus(),
                MaxBufferIndex = SasConstants.MaximumTransactionBufferIndex,
                CurrentCashableAmount = (ulong)_bank.QueryBalance(AccountType.Cashable).MillicentsToCents(),
                CurrentRestrictedAmount = (ulong)_bank.QueryBalance(AccountType.NonCash).MillicentsToCents(),
                CurrentNonRestrictedAmount = (ulong)_bank.QueryBalance(AccountType.Promo).MillicentsToCents(),
                CurrentGamingMachineTransferLimit = GetCurrentGamingMachineTransferLimit(),
                AssetNumber = _propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0)
            };

            var restrictedAmountNonZero = response.CurrentRestrictedAmount > 0;
            response.RestrictedExpiration = (uint)(restrictedAmountNonZero
                ? _ticketingCoordinator.TicketExpirationRestricted
                : 0);
            response.RestrictedPoolId = (ushort)(restrictedAmountNonZero ? _ticketingCoordinator.GetData().PoolId : 0);

            return response;
        }

        private AftTransferFlags GetHostCashoutStatus()
        {
            return _aftOptionsProvider.GetData().CurrentTransferFlags & AftTransferFlags.HostCashOutOptions;
        }

        private ulong GetCurrentGamingMachineTransferLimit()
        {
            var aftTransferLimit = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .TransferLimit.CentsToMillicents();

            var limit = _bank.Limit;
            var balance = _bank.QueryBalance();

            var bankTransferLimit = limit > balance ? limit - balance : 0;
            return (ulong)Math.Min(aftTransferLimit, bankTransferLimit).MillicentsToCents();
        }
    }
}