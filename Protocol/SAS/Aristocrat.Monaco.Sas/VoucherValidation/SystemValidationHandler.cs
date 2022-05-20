namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Storage.Models;
    using Storage.Repository;
    using Ticketing;

    /// <inheritdoc />
    public class SystemValidationHandler : BaseValidationHandler
    {
        private readonly ISasHost _sasHost;
        private readonly IHostValidationProvider _hostValidationProvider;

        /// <inheritdoc />
        public SystemValidationHandler(
            ISasHost sasHost,
            ITicketingCoordinator ticketingCoordinator,
            IPropertiesManager propertiesManager,
            ITransactionHistory transactionHistory,
            IHostValidationProvider hostValidationProvider,
            IStorageDataProvider<ValidationInformation> validationProvider)
            : base(SasValidationType.System, ticketingCoordinator, transactionHistory, propertiesManager, validationProvider)
        {
            _sasHost = sasHost ?? throw new ArgumentNullException(nameof(sasHost));
            _hostValidationProvider =
                hostValidationProvider ?? throw new ArgumentNullException(nameof(hostValidationProvider));
        }

        /// <inheritdoc />
        public override bool CanValidateTicketOutRequest(ulong amount, TicketType ticketType)
        {
            // System Validation can only happen when the host is online
            return _sasHost.IsHostOnline(SasGroup.Validation) && base.CanValidateTicketOutRequest(amount, ticketType);
        }

        /// <inheritdoc />
        public override async Task<TicketOutInfo> HandleTicketOutValidation(ulong amount, TicketType ticketType)
        {
            if (!CanValidate(ticketType) || await AnyTransactionPendingValidation())
            {
                return null;
            }

            var ticketExpirationDate = GetTicketExpirationDate(ticketType);
            if (!ValidExpirationDate(ticketType, ticketExpirationDate))
            {
                return null;
            }

            var validationResults = await _hostValidationProvider.GetValidationResults(amount, ticketType);
            return validationResults == null
                ? null
                : BuildTicketOut(
                    amount,
                    ticketType,
                    ticketExpirationDate,
                    $"{validationResults.SystemId:D2}{validationResults.ValidationNumber}");
        }

        /// <inheritdoc />
        public override Task HandleTicketOutCompleted(VoucherOutTransaction transaction) => Task.CompletedTask;

        /// <inheritdoc />
        public override Task HandleHandpayCompleted(HandpayTransaction transaction) => Task.CompletedTask;

        /// <inheritdoc />
        public override Task<TicketOutInfo> HandleHandPayValidation(ulong amount, HandPayType type)
        {
            var handPayTicketType = GetHandPayTicketType(type);

            // Bar-codes are always missing from handpays
            return Task.FromResult(
                BuildTicketOut(amount, handPayTicketType, GetTicketExpirationDate(handPayTicketType), string.Empty));
        }

        /// <inheritdoc />
        public override void Initialize()
        {
        }
    }
}