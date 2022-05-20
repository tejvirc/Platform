namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Kernel;
    using Kernel.Contracts;
    using log4net;

    /// <summary>Definition of the SasVoucherValidation class.</summary>
    public sealed class SasVoucherValidation : ITicketDataProvider, IVoucherValidator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISasHost _sasHost;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IIdProvider _idProvider;

        /// <summary>
        ///     Initializes a new instance of the SasVoucherValidation class.
        /// </summary>
        /// <param name="sasHost">The sas host</param>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="idProvider">The id provider.</param>
        public SasVoucherValidation(
            ISasHost sasHost,
            IPropertiesManager propertiesManager,
            IIdProvider idProvider)
        {
            _sasHost = sasHost ?? throw new ArgumentNullException(nameof(sasHost));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));

            TicketData = RestoreTicketData();
        }

        /// <summary>Gets or sets a value indicating whether it is in the operator menu.</summary>
        public bool InOperatorMenu { get; set; }

        /// <inheritdoc />
        public bool ReprintFailedVoucher => false;

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Debug("Initialized");
        }

        /// <inheritdoc />
        public string Name => typeof(SasVoucherValidation).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IVoucherValidator), typeof(ITicketDataProvider) };

        /// <inheritdoc />
        /// <summary>Gets the TicketData object.</summary>
        public TicketData TicketData { get; private set; }

        /// <inheritdoc />
        public void SetTicketData(TicketData newTicketData)
        {
            TicketData = newTicketData;

            UpdateTicketLocationProperty();
        }

        /// <inheritdoc />
        public bool CanValidateVouchersIn => _sasHost.IsHostOnline(SasGroup.Validation) &&
                                             _propertiesManager.GetValue(PropertyKey.VoucherIn, false);

        /// <inheritdoc />
        public bool CanCombineCashableAmounts => true; // SAS wants all cash to be combined into a single ticket

        /// <inheritdoc />
        public bool CanValidateVoucherOut(long amount, AccountType type)
        {
            return _sasHost.CanValidateTicketOutRequest(
                (ulong)amount,
                VoucherValidationConstants.AccountTypeToTicketTypeMap[type]);
        }

        /// <inheritdoc />
        public async Task<VoucherAmount> RedeemVoucher(VoucherInTransaction transaction)
        {
            if (_sasHost.IsRedemptionEnabled() && !InOperatorMenu)
            {
                await HandleVoucherIn(transaction);
            }
            else
            {
                Logger.Debug("ValidateVoucherIn: ticketing is disabled; rejecting");

                transaction.Amount = 0;
            }

            return await Task.FromResult<VoucherAmount>(null);
        }

        /// <inheritdoc />
        public Task StackedVoucher(VoucherInTransaction transaction)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<VoucherOutTransaction> IssueVoucher(
            VoucherAmount amount,
            AccountType type,
            Guid transactionId,
            TransferOutReason reason)
        {
            Logger.Debug($"ValidateVoucherOut(amount={amount.Amount},type={type},transactionId={transactionId})");

            var ticketType = VoucherValidationConstants.AccountTypeToTicketTypeMap[type];
            var request = await _sasHost.ValidateTicketOutRequest((ulong)amount.Amount, ticketType);

            if (request == null)
            {
                Logger.Info(
                    "Validation failed forcing the transaction to move on to the next available cashout device");
                return await Task.FromResult((VoucherOutTransaction)null);
            }

            return await Task.FromResult(
                new VoucherOutTransaction(
                    0,
                    request.Time,
                    (long)request.Amount,
                    type,
                    request.Barcode,
                    (int)request.TicketExpiration,
                    request.Pool,
                    "") { Reason = reason });
        }

        /// <inheritdoc />
        public void CommitVoucher(VoucherInTransaction transaction)
        {
            switch (transaction.State)
            {
                case VoucherState.Redeemed:
                    Logger.Debug("Voucher Accepted!");
                    _sasHost.TicketTransferComplete(transaction.TypeOfAccount);
                    break;
                case VoucherState.Rejected:
                    Logger.Debug("Voucher Not Accepted!");
                    _sasHost.TicketTransferFailed(transaction.Barcode, transaction.Exception, transaction.TransactionId);
                    break;
            }
        }

        /// <inheritdoc />
        public bool HostOnline => _sasHost.IsHostOnline(SasGroup.Validation);

        private void UpdateTicketLocationProperty()
        {
            _propertiesManager.SetProperty(VoucherValidationConstants.TicketLocationKey, TicketData.Location);
            _propertiesManager.SetProperty(VoucherValidationConstants.TicketAddressLine1Key, TicketData.Address1);
            _propertiesManager.SetProperty(VoucherValidationConstants.TicketAddressLine2Key, TicketData.Address2);
            _propertiesManager.SetProperty(
                AccountingConstants.TicketTitleNonCash,
                TicketData.RestrictedTicketTitle);
        }

        private static AccountType GetAccountTypeFromTransferCode(TicketTransferCode code)
        {
            switch (code)
            {
                case TicketTransferCode.ValidRestrictedPromotionalTicket:
                    return AccountType.NonCash;
                case TicketTransferCode.ValidNonRestrictedPromotionalTicket:
                    return AccountType.Promo;
                default:
                    return AccountType.Cashable;
            }
        }

        private async Task HandleVoucherIn(VoucherInTransaction transaction)
        {
            Logger.Debug($"ValidateVoucherIn: barcode: {transaction.Barcode}");
            var ticketInInfo = await _sasHost.ValidateTicketInRequest(transaction);

            if (ticketInInfo != null)
            {
                var accountType = GetAccountTypeFromTransferCode(ticketInInfo.TransferCode);
                Logger.Debug(
                    $"Amount={ticketInInfo.Amount}, Barcode={ticketInInfo.Barcode}, AccountType={accountType}");

                transaction.VoucherSequence = (int)_idProvider.GetNextLogSequence<SasVoucherValidation>();
                transaction.Amount = ((long)ticketInInfo.Amount).CentsToMillicents();
                transaction.TypeOfAccount = accountType;
                transaction.Exception = (int)ticketInInfo.GetExceptionCode();
            }
            else
            {
                Logger.Warn("Ticket In Denied by Sas Host");
            }
        }

        private TicketData RestoreTicketData()
        {
            return new TicketData
            {
                Location = _propertiesManager.GetValue(VoucherValidationConstants.TicketLocationKey, string.Empty),
                Address1 =
                    _propertiesManager.GetValue(VoucherValidationConstants.TicketAddressLine1Key, string.Empty),
                Address2 =
                    _propertiesManager.GetValue(VoucherValidationConstants.TicketAddressLine2Key, string.Empty),
                RestrictedTicketTitle = _propertiesManager.GetValue(
                    AccountingConstants.TicketTitleNonCash,
                    string.Empty)
            };
        }
    }
}