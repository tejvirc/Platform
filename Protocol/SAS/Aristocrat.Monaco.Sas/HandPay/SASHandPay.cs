namespace Aristocrat.Monaco.Sas.HandPay
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Kernel;
    using log4net;

    /// <summary>Definition of the SasHandPay class.</summary>
    public sealed class SasHandPay : IHandpayValidator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _properties;
        private readonly ISasHost _sasHost;
        private readonly ISasHandPayCommittedHandler _handPayCommittedHandler;

        /// <summary>
        ///     Initializes a new instance of the SasHandPay class.
        /// </summary>
        /// <param name="properties">The properties manager</param>
        /// <param name="sasHost">The SAS host</param>
        /// <param name="handPayCommittedHandler">The handpay committed handler</param>
        public SasHandPay(IPropertiesManager properties, ISasHost sasHost, ISasHandPayCommittedHandler handPayCommittedHandler)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _sasHost = sasHost ?? throw new ArgumentNullException(nameof(sasHost));
            _handPayCommittedHandler = handPayCommittedHandler ??
                                       throw new ArgumentNullException(nameof(handPayCommittedHandler));
        }

        /// <inheritdoc />
        public bool ValidateHandpay(
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            HandpayType handpayType)
        {
            Logger.Debug(
                $"Validating handpay (cashable={cashableAmount}, promo={promoAmount}, nonCash={nonCashAmount}, handpayType={handpayType})");
            return true;
        }

        /// <inheritdoc />
        public async Task RequestHandpay(HandpayTransaction transaction)
        {
            Logger.Debug(
                $"RequestHandpay(cashable={transaction.CashableAmount}, promo={transaction.PromoAmount}, nonCash={transaction.NonCashAmount}, transactionId={transaction.BankTransactionId})");

            // TODO When we have progressives set the correct handpay type
            var handPayType = HandPayType.CanceledCredit;
            if (transaction.HandpayType == HandpayType.BonusPay ||
                transaction.HandpayType == HandpayType.GameWin)
            {
                handPayType = HandPayType.NonProgressive;
            }

            await _handPayCommittedHandler.HandpayPending(transaction);
            var ticketOutInfo = await _sasHost.ValidateHandpayRequest(
                (ulong)(transaction.CashableAmount + transaction.PromoAmount + transaction.NonCashAmount),
                handPayType);

            transaction.Barcode = ticketOutInfo.Barcode;
            transaction.Expiration = (int)ticketOutInfo.TicketExpiration;
        }

        /// <inheritdoc />
        public Task HandpayKeyedOff(HandpayTransaction transaction)
        {
            _handPayCommittedHandler.HandPayReset(transaction);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool AllowLocalHandpay => true;

        /// <inheritdoc />
        public bool HostOnline => _sasHost.IsHostOnline(SasGroup.Validation);

        /// <inheritdoc />
        public bool LogTransactionRequired(ITransaction transaction)
        {
            if (transaction is HandpayTransaction handpay && handpay.IsCreditType())
            {
                return false;
            }

            return (bool)_properties.GetProperty(AccountingConstants.ValidateHandpays, false);
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public string Name => typeof(SasHandPay).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHandpayValidator) };
    }
}