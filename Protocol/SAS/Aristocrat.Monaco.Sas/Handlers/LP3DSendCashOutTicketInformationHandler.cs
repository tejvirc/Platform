namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     The handler for LP 3D Send Cash Out Ticket Information
    /// </summary>
    public class
        LP3DSendCashOutTicketInformationHandler : ISasLongPollHandler<LongPollSendCashOutTicketInformationResponse,
            LongPollData>
    {
        private readonly ITransactionHistory _transactionHistory;
        private readonly bool _isNoneValidationType;

        private const long DefaultTransactionAmount = 0;
        private const string DefaultValidationNumber = "0";

        /// <summary>
        ///     Constructs the handler
        /// </summary>
        /// <param name="transactionHistory">The transaction history provider</param>
        /// <param name="propertiesManager">The properties manager provider</param>
        public LP3DSendCashOutTicketInformationHandler(
            ITransactionHistory transactionHistory,
            IPropertiesManager propertiesManager)
        {
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));

            _isNoneValidationType =
                (propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager)))
                .GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ValidationType == SasValidationType.None;
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendCashOutTicketInformation };

        /// <inheritdoc />
        public LongPollSendCashOutTicketInformationResponse Handle(LongPollData data)
        {
            // Don't respond to 3D commands when not configured for None validation
            if (!_isNoneValidationType)
            {
                return null;
            }

            var lastTransaction = _transactionHistory.RecallTransactions()
                .FirstOrDefault(x => x is HandpayTransaction || x is VoucherOutTransaction);

            long transactionAmount;
            string validationNumber;
            switch (lastTransaction)
            {
                case HandpayTransaction handPay:
                    transactionAmount = handPay.TransactionAmount;
                    validationNumber = handPay.Barcode;
                    break;
                case VoucherOutTransaction voucherOut:
                    transactionAmount = voucherOut.TransactionAmount;
                    validationNumber = voucherOut.Barcode;
                    break;
                default:
                    transactionAmount = DefaultTransactionAmount;
                    validationNumber = DefaultValidationNumber;
                    break;
            }

            return new LongPollSendCashOutTicketInformationResponse(
                Convert.ToInt64(validationNumber),
                transactionAmount.MillicentsToCents());
        }
    }
}