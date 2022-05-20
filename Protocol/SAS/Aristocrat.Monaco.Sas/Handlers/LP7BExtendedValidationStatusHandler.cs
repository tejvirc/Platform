namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Storage.Models;
    using Storage.Repository;
    using Ticketing;
    using VoucherValidation;
    using IPrinter = Hardware.Contracts.Printer.IPrinter;

    /// <inheritdoc />
    public class LP7BExtendedValidationStatusHandler : ISasLongPollHandler<ExtendedValidationStatusResponse, ExtendedValidationStatusData>
    {
        private static readonly IList<ValidationControlStatus> ValidationControlStatuses =
            Enum.GetValues(typeof(ValidationControlStatus)).Cast<ValidationControlStatus>().ToList();

        private readonly IPropertiesManager _propertiesManager;
        private readonly IStorageDataProvider<ValidationInformation> _validationProvider;
        private readonly IPersistentStorageManager _persistentStorageManager;
        private readonly ITicketingCoordinator _ticketingCoordinator;
        private readonly ITicketDataProvider _ticketDataProvider;
        private readonly ITransactionHistory _transactionHistory;
        private readonly SasValidationHandlerFactory _validationFactory;
        private readonly IPrinter _printer;
        private readonly INoteAcceptor _noteAcceptor;

        /// <summary>
        ///     Creates the LP7BExtendedValidationStatusHandler instance
        /// </summary>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="validationProvider">An instance of <see cref="IStorageDataProvider{ValidationInformation}"/></param>
        /// <param name="persistentStorageManager">The persistent storage manager</param>
        /// <param name="ticketingCoordinator">The ticketing coordinator</param>
        /// <param name="ticketDataProvider">The ticket data provider</param>
        /// <param name="transactionHistory">The transaction history</param>
        /// <param name="validationFactory">The validation factory</param>
        public LP7BExtendedValidationStatusHandler(
            IPropertiesManager propertiesManager,
            IStorageDataProvider<ValidationInformation> validationProvider,
            IPersistentStorageManager persistentStorageManager,
            ITicketingCoordinator ticketingCoordinator,
            ITicketDataProvider ticketDataProvider,
            ITransactionHistory transactionHistory,
            SasValidationHandlerFactory validationFactory)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _validationProvider = validationProvider ?? throw new ArgumentNullException(nameof(validationProvider));
            _persistentStorageManager = persistentStorageManager ??
                                        throw new ArgumentNullException(nameof(persistentStorageManager));
            _ticketingCoordinator = ticketingCoordinator ??
                                    throw new ArgumentNullException(nameof(ticketingCoordinator));
            _ticketDataProvider = ticketDataProvider ?? throw new ArgumentNullException(nameof(ticketDataProvider));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _validationFactory = validationFactory ?? throw new ArgumentNullException(nameof(validationFactory));
            var serviceManager = ServiceManager.GetInstance();
            _printer = serviceManager.TryGetService<IPrinter>();
            _noteAcceptor = serviceManager.TryGetService<INoteAcceptor>();
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.ExtendedValidationStatus
        };

        /// <inheritdoc />
        public ExtendedValidationStatusResponse Handle(ExtendedValidationStatusData data)
        {
            var validationMode = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ValidationType;
            var cashableExpirationDays = (int)_ticketingCoordinator.TicketExpirationCashable;
            var restrictedExpirationDays = (int)_ticketingCoordinator.DefaultTicketExpirationRestricted;
            var assertNumber = _propertiesManager.GetValue(ApplicationConstants.MachineId, (uint)0);

            if (validationMode != SasValidationType.SecureEnhanced &&
                validationMode != SasValidationType.System)
            {
                // Only applies to system and secure enhanced
                return new ExtendedValidationStatusResponse(
                    assertNumber,
                    ValidationControlStatus.None,
                    cashableExpirationDays,
                    restrictedExpirationDays);
            }

            if ((data.ControlStatus & ValidationControlStatus.SecureEnhancedConfiguration) == 0 &&
                (data.ControlMask & ValidationControlStatus.SecureEnhancedConfiguration) > 0 &&
                validationMode == SasValidationType.SecureEnhanced)
            {
                ClearValidationInformation(validationMode).FireAndForget();
                return new ExtendedValidationStatusResponse(
                    assertNumber,
                    ValidationControlStatus.None,
                    SasConstants.MaxTicketExpirationDays,
                    SasConstants.MaxTicketExpirationDays);
            }

            // Clear the secure enhanced configuration as this is handled as a special case above
            data.ControlMask &= ~ValidationControlStatus.SecureEnhancedConfiguration;
            // TODO: We do not currently support this so just disable control;
            // Handpays, if able to be validated, will be no matter what is sent via LP7B (incorrect behavior for now):
            data.ControlMask &= ~ValidationControlStatus.ValidateHandPays;
            var (persistData, validationControl) = HandleValidationControlStatuses(data, validationMode);
            if (data.CashableExpirationDate != 0)
            {
                cashableExpirationDays = data.CashableExpirationDate;
                persistData = true;
            }

            if (data.RestrictedExpirationDate != 0)
            {
                restrictedExpirationDays = data.RestrictedExpirationDate;
                persistData = true;
            }

            if (persistData)
            {
                PersistData(
                    cashableExpirationDays,
                    restrictedExpirationDays,
                    validationControl).FireAndForget();
            }

            return new ExtendedValidationStatusResponse(
                assertNumber,
                GetCurrentValidationControlStatuses(validationMode, validationControl),
                cashableExpirationDays,
                restrictedExpirationDays);
        }

        private (bool persistData, ValidationControlStatus currentStatus) HandleValidationControlStatuses(
            ExtendedValidationStatusData data,
            SasValidationType validationType)
        {
            var result = GetConfiguredValidationStatus(validationType);
            var initialValidationStatus = GetSupportedValidationStatus(validationType);
            var persistData = false;

            foreach (var status in ValidationControlStatuses)
            {
                if ((initialValidationStatus & status) == 0 || // If we don't support the flag we cannot set the configuration so just ignore it
                    (data.ControlMask & status) == 0 || // If they don't set the control mask just ignore it
                    (result & status) == (data.ControlStatus & status)) // If they are not configuring anything different just ignore it
                {
                    continue;
                }

                result = (data.ControlStatus & status) > 0 ? result | status : result & ~status;

                if (status == ValidationControlStatus.PrintRestrictedTickets)
                {
                    // This cant be directly configured but since we don't meter the source separately
                    // set it the same as restricted credits for the response
                    result = (result & status) > 0
                        ? result | ValidationControlStatus.PrintForeignRestrictedTickets
                        : result & ~ValidationControlStatus.PrintForeignRestrictedTickets;
                }

                persistData = true;
            }

            return (persistData, result);
        }

        private ValidationControlStatus GetSupportedValidationStatus(SasValidationType validationType)
        {
            return GetSecureEnhancedControlStatuses(validationType) | GetTicketingValidationControlStatuses();
        }

        private ValidationControlStatus GetTicketingValidationControlStatuses()
        {
            if (!_propertiesManager.GetValue(SasProperties.PrinterAsCashOutDeviceSupportedKey, false))
            {
                return ValidationControlStatus.None;
            }

            var controlStatus = ValidationControlStatus.UsePrinterAsCashoutDevice;
            if (_propertiesManager.GetValue(SasProperties.ForeignRestrictedTicketsSupportedKey, false))
            {
                controlStatus |= ValidationControlStatus.PrintForeignRestrictedTickets;
            }

            if (_propertiesManager.GetValue(SasProperties.RestrictedTicketsSupportedKey, false))
            {
                controlStatus |= ValidationControlStatus.PrintRestrictedTickets;
            }

            if (_propertiesManager.GetValue(SasProperties.TicketRedemptionSupportedKey, false))
            {
                controlStatus |= ValidationControlStatus.TicketRedemption;
            }

            return controlStatus;
        }

        private ValidationControlStatus GetSecureEnhancedControlStatuses(SasValidationType validationType)
        {
            if (validationType != SasValidationType.SecureEnhanced)
            {
                return ValidationControlStatus.None;
            }

            var controlStatus = ValidationControlStatus.SecureEnhancedConfiguration;
            if (_propertiesManager.GetValue(SasProperties.HandPayValidationSupportedKey, false))
            {
                controlStatus |= ValidationControlStatus.ValidateHandPays;
                if (_propertiesManager.GetValue(SasProperties.PrinterAsHandPayDeviceSupportedKey, false))
                {
                    controlStatus |= ValidationControlStatus.UsePrinterAsHandPayDevice;
                }
            }

            return controlStatus;
        }

        private ValidationControlStatus GetConfiguredValidationStatus(SasValidationType validationType)
        {
            var status = ValidationControlStatus.None;
            if (_propertiesManager.GetValue(AccountingConstants.VoucherOut, false))
            {
                status |= ValidationControlStatus.UsePrinterAsCashoutDevice;
            }

            if (_propertiesManager.GetValue(AccountingConstants.VoucherOutNonCash, false))
            {
                status |= ValidationControlStatus.PrintRestrictedTickets;

                // We don't meter the source separately so these print the same for all restricted credits
                status |= ValidationControlStatus.PrintForeignRestrictedTickets;
            }

            if (_propertiesManager.GetValue(AccountingConstants.ValidateHandpays, false))
            {
                status |= ValidationControlStatus.ValidateHandPays;
            }

            if (_propertiesManager.GetValue(AccountingConstants.EnableReceipts, false))
            {
                status |= ValidationControlStatus.UsePrinterAsHandPayDevice;
            }

            if (_propertiesManager.GetValue(PropertyKey.VoucherIn, false))
            {
                status |= ValidationControlStatus.TicketRedemption;
            }

            if (validationType == SasValidationType.SecureEnhanced)
            {
                status |= ValidationControlStatus.SecureEnhancedConfiguration;
            }

            return status;
        }

        private ValidationControlStatus GetCurrentValidationControlStatuses(
            SasValidationType validationType,
            ValidationControlStatus currentValidationStatus)
        {
            var result = ValidationControlStatus.None;
            var printerEnabled = _printer?.CanPrint ?? false;
            var noteAcceptorEnabled = _noteAcceptor?.Enabled ?? false;

            if (validationType == SasValidationType.SecureEnhanced)
            {
                if (!_validationProvider.GetData().ValidationConfigured)
                {
                    return ValidationControlStatus.None;
                }

                result |= currentValidationStatus & ValidationControlStatus.SecureEnhancedConfiguration;
                if ((currentValidationStatus & ValidationControlStatus.ValidateHandPays) > 0)
                {
                    result |= ValidationControlStatus.ValidateHandPays;
                    if (printerEnabled)
                    {
                        result |= currentValidationStatus & ValidationControlStatus.UsePrinterAsHandPayDevice;
                    }
                }
            }

            if (printerEnabled && !_transactionHistory.AnyPendingHostAcknowledged())
            {
                result |= currentValidationStatus & ValidationControlStatus.UsePrinterAsCashoutDevice;
                result |= currentValidationStatus & ValidationControlStatus.PrintRestrictedTickets;
                result |= currentValidationStatus & ValidationControlStatus.PrintForeignRestrictedTickets;
            }

            if (noteAcceptorEnabled)
            {
                result |= currentValidationStatus & ValidationControlStatus.TicketRedemption;
            }

            return result;
        }

        private async Task ClearValidationInformation(SasValidationType validationType)
        {
            var validationInformation = _validationProvider.GetData();
            validationInformation.ExtendedTicketDataStatus = TicketDataStatus.InvalidData;
            validationInformation.ExtendedTicketDataSet = false;
            validationInformation.ValidationConfigured = false;
            await _validationProvider.Save(validationInformation);

            using (var scopedTransaction = _persistentStorageManager.ScopedTransaction())
            {
                _ticketingCoordinator.ValidationConfigurationCancelled();
                var ticketDataReset = new TicketData
                {
                    Address1 = _propertiesManager.GetValue(SasProperties.DefaultAddressLine1Key, string.Empty),
                    Address2 = _propertiesManager.GetValue(SasProperties.DefaultAddressLine2Key, string.Empty),
                    DebitTicketTitle = _propertiesManager.GetValue(SasProperties.DefaultDebitTitleKey, string.Empty),
                    Location = _propertiesManager.GetValue(SasProperties.DefaultLocationKey, string.Empty),
                    RestrictedTicketTitle = _propertiesManager.GetValue(
                        SasProperties.DefaultRestrictedTitleKey,
                        string.Empty)
                };
                
                _ticketDataProvider.SetTicketData(ticketDataReset);
                var supportedStatus = GetSupportedValidationStatus(validationType);
                _propertiesManager.SetProperty(AccountingConstants.VoucherOut, (supportedStatus & ValidationControlStatus.UsePrinterAsCashoutDevice) != 0);
                _propertiesManager.SetProperty(AccountingConstants.VoucherOutNonCash, (supportedStatus & ValidationControlStatus.PrintRestrictedTickets) != 0);
                _propertiesManager.SetProperty(AccountingConstants.ValidateHandpays, (supportedStatus & ValidationControlStatus.ValidateHandPays) != 0);
                _propertiesManager.SetProperty(AccountingConstants.EnableReceipts, (supportedStatus & ValidationControlStatus.UsePrinterAsHandPayDevice) != 0);
                _propertiesManager.SetProperty(PropertyKey.VoucherIn, (supportedStatus & ValidationControlStatus.TicketRedemption) != 0);

                scopedTransaction.Complete();
            }

            _validationFactory.GetValidationHandler()?.Initialize();
        }

        private async Task PersistData(
            int cashableExpirationDays,
            int restrictedExpirationDays,
            ValidationControlStatus validationStatus)
        {
            var ticketStorageData = _ticketingCoordinator.GetData();
            ticketStorageData.CashableTicketExpiration = cashableExpirationDays;
            ticketStorageData.SetRestrictedExpiration(ExpirationOrigin.Independent, restrictedExpirationDays);
            await _ticketingCoordinator.Save(ticketStorageData);

            using (var scopedTransaction = _persistentStorageManager.ScopedTransaction())
            {
                _propertiesManager.SetProperty(PropertyKey.VoucherIn, (validationStatus & ValidationControlStatus.TicketRedemption) != 0);
                _propertiesManager.SetProperty(AccountingConstants.VoucherOut, (validationStatus & ValidationControlStatus.UsePrinterAsCashoutDevice) != 0);
                _propertiesManager.SetProperty(AccountingConstants.VoucherOutNonCash, (validationStatus & ValidationControlStatus.PrintRestrictedTickets) != 0);
                _propertiesManager.SetProperty(AccountingConstants.EnableReceipts, (validationStatus & ValidationControlStatus.UsePrinterAsHandPayDevice) != 0);
                _propertiesManager.SetProperty(AccountingConstants.ValidateHandpays, (validationStatus & ValidationControlStatus.ValidateHandPays) != 0);
                scopedTransaction.Complete();
            }
        }
    }
}