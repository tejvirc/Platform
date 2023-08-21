namespace Aristocrat.Monaco.Sas.Ticketing
{
    using System;
    using System.Data;
    using System.Linq;
    using Accounting.Contracts;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Kernel;
    using Protocol.Common.Storage.Entity;
    using Storage.Models;
    using Storage.Repository;

    /// <summary>
    ///     A class used to coordinate any operation for ticketing
    /// </summary>
    public class TicketingCoordinator : StorageDataProvider<TicketStorageData>, ITicketingCoordinator
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPropertiesManager _propertiesManager;

        private ulong TicketExpiration => GetExpirationOrDefault(
            GetData().TicketExpiration,
            AccountingConstants.VoucherOutExpirationDays);

        /// <inheritdoc />
        public ulong TicketExpirationCashable => GetTicketExpirationCashable();

        /// <inheritdoc />
        public ulong TicketExpirationRestricted => GetExpirationOrDefault(
            GetData().GetHighestPriorityExpiration(),
            AccountingConstants.VoucherOutNonCashExpirationDays);

        /// <inheritdoc />
        public ulong DefaultTicketExpirationRestricted => GetExpirationOrDefault(
            GetData().GetDefaultRestrictedExpiration(),
            AccountingConstants.VoucherOutNonCashExpirationDays);

        /// <summary>
        ///     Constructs an instance of the TicketingCoordinator class
        /// </summary>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="propertiesManager">The properties manager</param>
        public TicketingCoordinator(
            IUnitOfWorkFactory unitOfWorkFactory,
            IPropertiesManager propertiesManager) : base(unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public void ValidationConfigurationCancelled()
        {
            using var work = _unitOfWorkFactory.Create();
            work.BeginTransaction(IsolationLevel.Serializable);
            var storageData = GetData();
            storageData.TicketExpiration = TicketStorageData.ExpirationNotSet;
            storageData.CashableTicketExpiration = TicketStorageData.ExpirationNotSet;
            storageData.PoolId = 0;
            foreach (var origin in Enum.GetValues(typeof(ExpirationOrigin)).Cast<ExpirationOrigin>()
                .Where(x => x != ExpirationOrigin.EgmDefault && x != ExpirationOrigin.SizeOfExpirationOrigin))
            {
                storageData.CancelExpiration(origin);
            }

            Save(storageData);
            work.Commit();
        }

        /// <inheritdoc />
        public void RestrictedCreditsZeroed()
        {
            using var work = _unitOfWorkFactory.Create();
            work.BeginTransaction(IsolationLevel.Serializable);
            var storageData = GetData();
            storageData.PoolId = 0;
            storageData.CancelExpiration(ExpirationOrigin.Credits);
            Save(storageData, work);
            work.Commit();
        }

        private ulong GetExpirationOrDefault(int expirationDate, string expirationProperty)
        {
            var expiration = (ulong)(expirationDate == TicketStorageData.ExpirationNotSet
                ? _propertiesManager.GetValue(expirationProperty, 0)
                : expirationDate);
            return expiration == 0 ? SasConstants.MaxTicketExpirationDays : expiration;
        }

        private ulong GetTicketExpirationCashable()
        {
            // Fixed Expiration, No change allowed
            if (!_propertiesManager.GetValue(AccountingConstants.EditableExpiration, true))
            {
                return (ulong)_propertiesManager.GetValue(AccountingConstants.VoucherOutExpirationDays, 0);
            }

            var storageData = GetData();
            return storageData.CashableTicketExpiration == TicketStorageData.ExpirationNotSet
                ? TicketExpiration
                : (ulong)storageData.CashableTicketExpiration;
        }
    }
}
