namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;
    using Storage.Models;
    using Storage.Repository;
    using Ticketing;

    /// <inheritdoc />
    public abstract class BaseValidationHandler : IValidationHandler
    {
        private const int ValidationRetryWaitTime = 200;
        private const int MaxValidationRetryCount = 5;

        /// <inheritdoc />
        public SasValidationType ValidationType { get; }

        /// <summary>
        ///     Gets the logger for the validation handler
        /// </summary>
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Gets the validation provider
        /// </summary>
        protected IStorageDataProvider<ValidationInformation> ValidationProvider { get; }

        /// <summary>
        ///     Gets the ticketing coordinator
        /// </summary>
        protected ITicketingCoordinator TicketingCoordinator { get; }

        /// <summary>
        ///     Gets the transaction history
        /// </summary>
        protected ITransactionHistory TransactionHistory { get; }

        /// <summary>
        ///     Gets the properties manager
        /// </summary>
        protected IPropertiesManager PropertiesManager { get; }

        /// <summary>
        ///     Creates the base validation handler
        /// </summary>
        /// <param name="validationType">The validation handler type to create</param>
        /// <param name="ticketingCoordinator">The ticketing coordinator</param>
        /// <param name="transactionHistory">The transaction history provider</param>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="validationProvider">The validation data provider</param>
        protected BaseValidationHandler(
            SasValidationType validationType,
            ITicketingCoordinator ticketingCoordinator,
            ITransactionHistory transactionHistory,
            IPropertiesManager propertiesManager,
            IStorageDataProvider<ValidationInformation> validationProvider)
        {
            TicketingCoordinator = ticketingCoordinator ?? throw new ArgumentNullException(nameof(ticketingCoordinator));
            TransactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            PropertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            ValidationProvider = validationProvider ?? throw new ArgumentNullException(nameof(validationProvider));
            ValidationType = validationType;
        }

        /// <inheritdoc />
        public virtual bool CanValidateTicketOutRequest(ulong amount, TicketType ticketType)
        {
            return CanValidate(ticketType) && ValidExpirationDate(ticketType, GetTicketExpirationDate(ticketType));
        }

        /// <inheritdoc />
        public abstract Task<TicketOutInfo> HandleTicketOutValidation(ulong amount, TicketType ticketType);

        /// <inheritdoc />
        public abstract Task HandleTicketOutCompleted(VoucherOutTransaction transaction);

        /// <inheritdoc />
        public abstract Task HandleHandpayCompleted(HandpayTransaction transaction);

        /// <inheritdoc />
        public abstract Task<TicketOutInfo> HandleHandPayValidation(ulong amount, HandPayType type);

        /// <inheritdoc />
        public abstract void Initialize();

        /// <summary>
        ///     Checks whether or not the expiration date for the provided ticket type is valid
        /// </summary>
        /// <param name="ticketType">The ticket type to check the expiration date for</param>
        /// <param name="ticketExpirationDate">The ticket expiration date to check</param>
        /// <returns></returns>
        protected static bool ValidExpirationDate(TicketType ticketType, int ticketExpirationDate)
        {
            if (ticketType != TicketType.Restricted ||
                ticketExpirationDate <= SasConstants.MaxTicketExpirationDays)
            {
                return true;
            }

            var date = Utilities.FromSasDate((ulong)ticketExpirationDate);
            return (DateTime.Now.Date <= date);
        }

        /// <summary>
        ///     Whether or not the ticket type can be validated by our system
        /// </summary>
        /// <param name="ticketType">The ticket type to validate</param>
        /// <returns>Whether or not we can validate the requested ticket type</returns>
        protected bool CanValidate(TicketType ticketType)
        {
            // Can we use the printer as the cashout device?
            return PropertiesManager.GetValue(AccountingConstants.VoucherOut, false) &&
                   (ticketType != TicketType.Restricted ||
                    PropertiesManager.GetValue(AccountingConstants.VoucherOutNonCash, false));
        }

        /// <summary>
        ///     Creates the ticket out info
        /// </summary>
        /// <param name="amount">The amount for this ticket</param>
        /// <param name="ticketType">The ticket type to use</param>
        /// <param name="ticketExpirationDate">The ticket expiration to use</param>
        /// <param name="barcode">The generated barcode</param>
        /// <returns>The created ticket out info</returns>
        protected TicketOutInfo BuildTicketOut(
            ulong amount,
            TicketType ticketType,
            int ticketExpirationDate,
            string barcode)
        {
            return new TicketOutInfo
            {
                Amount = amount,
                Time = DateTime.UtcNow,
                Barcode = barcode,
                ValidationType = GetTicketValidationType(ticketType),
                Pool = GetPoolId(ticketType),
                TicketExpiration = (uint)ticketExpirationDate
            };
        }

        /// <summary>
        ///     Gets the pool id for the requested ticket type
        /// </summary>
        /// <param name="ticketType">The ticket type to get the pool id for</param>
        /// <returns>The pool id for the provided ticket type</returns>
        protected ushort GetPoolId(TicketType ticketType)
        {
            return (ushort)(ticketType == TicketType.Restricted
                ? TicketingCoordinator.GetData().PoolId
                : SasConstants.NoPoolIdSet);
        }

        /// <summary>
        ///     Gets the ticket validation type
        /// </summary>
        /// <param name="ticketType">The ticket type to get the validation type for</param>
        /// <returns>The ticket validation type</returns>
        protected TicketValidationType GetTicketValidationType(TicketType ticketType)
        {
            switch (ticketType)
            {
                case TicketType.CashOutReceipt:
                    return PropertiesManager.GetValue(AccountingConstants.EnableReceipts, false) &&
                           PropertiesManager.GetValue(AccountingConstants.ValidateHandpays, false)
                        ? TicketValidationType.HandPayFromCashOutReceiptPrinted
                        : TicketValidationType.HandPayFromCashOutNoReceipt;
                case TicketType.Jackpot:
                case TicketType.JackpotOffline:
                    return TicketValidationType.HandPayFromWinNoReceipt;
                case TicketType.JackpotReceipt:
                    return PropertiesManager.GetValue(AccountingConstants.EnableReceipts, false) &&
                           PropertiesManager.GetValue(AccountingConstants.ValidateHandpays, false)
                        ? TicketValidationType.HandPayFromWinReceiptPrinted
                        : TicketValidationType.HandPayFromWinNoReceipt;
                case TicketType.HandPayValidated:
                    return TicketValidationType.HandPayFromCashOutNoReceipt;
                case TicketType.Restricted:
                    return TicketValidationType.RestrictedPromotionalTicketFromCashOut;
                default:
                    return TicketValidationType.CashableTicketFromCashOutOrWin;
            }
        }

        /// <summary>
        ///     Gets the handpay ticket type
        /// </summary>
        /// <param name="handPayType">The type of handpay to get the handpay ticket type for</param>
        /// <returns>The handpay ticket type</returns>
        protected TicketType GetHandPayTicketType(HandPayType handPayType)
        {
            switch (handPayType)
            {
                case HandPayType.CanceledCredit:
                    return PropertiesManager.GetValue(AccountingConstants.EnableReceipts, false) &&
                           PropertiesManager.GetValue(AccountingConstants.ValidateHandpays, false)
                        ? TicketType.CashOutReceipt
                        : TicketType.HandPayValidated;
                case HandPayType.NonProgressiveNoReceipt:
                    return TicketType.Jackpot;
                default:
                    return TicketType.JackpotReceipt;
            }
        }

        /// <summary>
        ///     Gets the ticket expiration date for the provided ticket type
        /// </summary>
        /// <param name="ticketType">The ticket type to get the expiration date for</param>
        /// <returns>The expiration date to get the ticket type for</returns>
        protected int GetTicketExpirationDate(TicketType ticketType)
        {
            switch (ticketType)
            {
                case TicketType.CashOut:
                case TicketType.CashOutOffline:
                case TicketType.CashOutReceipt:
                case TicketType.Jackpot:
                case TicketType.JackpotOffline:
                case TicketType.JackpotReceipt:
                case TicketType.HandPayValidated:
                    return (int)TicketingCoordinator.TicketExpirationCashable;
                case TicketType.Restricted:
                    return (int)TicketingCoordinator.TicketExpirationRestricted;
                default:
                    Logger.Error($"TicketType {ticketType} is not handled. Expiration(incorrectly ?) left as zero");
                    return 0;
            }
        }

        /// <summary>
        ///     Gets whether or not there are any pending validation requests
        /// </summary>
        /// <returns>Whether or not there are any pending validation requests</returns>
        protected async Task<bool> AnyTransactionPendingValidation()
        {
            if (!TransactionHistory.AnyPendingHostAcknowledged())
            {
                return false;
            }

            for (var currentRetry = 0; currentRetry < MaxValidationRetryCount; ++currentRetry)
            {
                await Task.Delay(ValidationRetryWaitTime);
                if (!TransactionHistory.AnyPendingHostAcknowledged())
                {
                    return false;
                }
            }

            return TransactionHistory.AnyPendingHostAcknowledged();
        }
    }
}