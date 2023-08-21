namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Protocol.Common.Storage.Entity;
    using Storage.Models;
    using Storage.Repository;
    using Ticketing;

    /// <inheritdoc />
    public class SetTicketDataHandler :
        ISasLongPollHandler<LongPollReadSingleValueResponse<TicketDataStatus>, SetTicketData>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IStorageDataProvider<ValidationInformation> _validationProvider;
        private readonly ITicketDataProvider _ticketDataProvider;
        private readonly ITicketingCoordinator _ticketingCoordinator;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IHostAcknowledgementHandler _handler;
        private readonly IHostAcknowledgementHandler _nullHandler = new HostAcknowledgementHandler();

        /// <summary>
        ///     Creates the SetTicketDataHandler instance
        /// </summary>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="validationProvider"></param>
        /// <param name="ticketDataProvider">The ticket data provider</param>
        /// <param name="ticketingCoordinator">The ticketing coordinator</param>
        /// <param name="unitOfWorkFactory"></param>
        public SetTicketDataHandler(
            IPropertiesManager propertiesManager,
            IStorageDataProvider<ValidationInformation> validationProvider,
            ITicketDataProvider ticketDataProvider,
            ITicketingCoordinator ticketingCoordinator,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _validationProvider = validationProvider ?? throw new ArgumentNullException(nameof(validationProvider));
            _ticketDataProvider = ticketDataProvider ?? throw new ArgumentNullException(nameof(ticketDataProvider));
            _ticketingCoordinator = ticketingCoordinator ?? throw new ArgumentNullException(nameof(ticketingCoordinator));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

            _handler = new HostAcknowledgementHandler
            {
                ImpliedNackHandler =
                    () => _propertiesManager.SetProperty(SasProperties.ExtendedTicketDataStatusClearPending, false),

                ImpliedAckHandler = () =>
                {
                    if (_propertiesManager.GetValue(SasProperties.ExtendedTicketDataStatusClearPending, false))
                    {
                        _propertiesManager.SetProperty(SasProperties.ExtendedTicketDataStatusClearPending, false);
                        var validationInformation = _validationProvider.GetData();
                        validationInformation.ExtendedTicketDataStatus = TicketDataStatus.InvalidData;
                        _validationProvider.Save(validationInformation).FireAndForget();
                    }
                }
            };
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SetExtendedTicketData,
            LongPoll.SetTicketData
        };

        /// <inheritdoc />
        public LongPollReadSingleValueResponse<TicketDataStatus> Handle(SetTicketData data)
        {
            var handler = _nullHandler;
            var validationInformation = _validationProvider.GetData();
            var hasExtendedTicketData = validationInformation.ExtendedTicketDataSet;
            var ticketDataStatus = TicketDataStatus.InvalidData;
            if (data.ValidTicketData && (data.IsExtendTicketData || !hasExtendedTicketData))
            {
                var (currentTicketData, updated) = GetUpdatedTicketData(data);
                if (updated || data.SetExpirationDate)
                {
                    ticketDataStatus = TicketDataStatus.ValidData;
                    Task.Run(() => UpdatePersistence(currentTicketData, data, ticketDataStatus));
                }

                if (!data.BroadcastPoll)
                {
                    _propertiesManager.SetProperty(SasProperties.ExtendedTicketDataStatusClearPending, true);
                    handler = _handler;
                }
            }
            else
            {
                validationInformation.ExtendedTicketDataStatus = ticketDataStatus;
                _validationProvider.Save(validationInformation).FireAndForget();
            }

            return new LongPollReadSingleValueResponse<TicketDataStatus>(ticketDataStatus)
            {
                Handlers = handler
            };
        }

        private void UpdatePersistence(TicketData currentTicketData, SetTicketData ticketData, TicketDataStatus ticketDataStatus)
        {
            using var work = _unitOfWorkFactory.Create();
            work.BeginTransaction(IsolationLevel.Serializable);
            if (ticketData.SetExpirationDate)
            {
                var storageData = _ticketingCoordinator.GetData();
                storageData.SetRestrictedExpiration(
                    ExpirationOrigin.Combined,
                    ticketData.ExpirationDate > 0
                        ? ticketData.ExpirationDate
                        : SasConstants.MaxTicketExpirationDays);
                storageData.TicketExpiration = ticketData.ExpirationDate > 0
                    ? ticketData.ExpirationDate
                    : SasConstants.MaxTicketExpirationDays;
                _ticketingCoordinator.Save(storageData, work);
            }

            _ticketDataProvider.SetTicketData(currentTicketData);
            var information = _validationProvider.GetData();
            if (ticketData.IsExtendTicketData)
            {
                information.ExtendedTicketDataSet = true;
            }

            information.ExtendedTicketDataStatus = ticketDataStatus;
            _validationProvider.Save(information, work);
            work.Commit();
        }

        private (TicketData ticketData, bool updated) GetUpdatedTicketData(SetTicketData data)
        {
            var currentTicketData = _ticketDataProvider.TicketData;
            var updated = false;
            if (data.Location != null)
            {
                updated = true;
                currentTicketData.Location = (data.Location == string.Empty)
                    ? _propertiesManager.GetValue(
                        SasProperties.DefaultLocationKey,
                        string.Empty)
                    : data.Location;
            }

            if (data.Address1 != null)
            {
                updated = true;
                currentTicketData.Address1 = (data.Address1 == string.Empty)
                    ? _propertiesManager.GetValue(
                        SasProperties.DefaultAddressLine1Key,
                        string.Empty)
                    : data.Address1;
            }

            if (data.Address2 != null)
            {
                updated = true;
                currentTicketData.Address2 = (data.Address2 == string.Empty)
                    ? _propertiesManager.GetValue(
                        SasProperties.DefaultAddressLine2Key,
                        string.Empty)
                    : data.Address2;
            }

            if (data.RestrictedTicketTitle != null)
            {
                updated = true;
                currentTicketData.RestrictedTicketTitle = (data.RestrictedTicketTitle == string.Empty)
                    ? _propertiesManager.GetValue(
                        SasProperties.DefaultRestrictedTitleKey,
                        string.Empty)
                    : data.RestrictedTicketTitle;
            }

            if (data.DebitTicketTitle != null)
            {
                updated = true;
                currentTicketData.DebitTicketTitle = (data.DebitTicketTitle == string.Empty)
                    ? _propertiesManager.GetValue(
                        SasProperties.DefaultDebitTitleKey,
                        string.Empty)
                    : data.DebitTicketTitle;
            }

            return (currentTicketData, updated);
        }
    }
}