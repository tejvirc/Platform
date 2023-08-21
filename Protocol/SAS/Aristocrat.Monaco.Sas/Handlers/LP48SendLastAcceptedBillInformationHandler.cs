namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Hardware.Contracts.NoteAcceptor;

    /// <summary>
    ///     The handler for LP 48 Send Last Accepted Bill Information
    /// </summary>
    public class LP48SendLastAcceptedBillInformationHandler : ISasLongPollHandler<SendLastAcceptedBillInformationResponse, LongPollData>
    {
        private readonly IMeterManager _meterManager;
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Creates an Instance of the LP48SendLastAcceptedBillInformationHandler
        /// </summary>
        /// <param name="meterManager">A reference to the meter manager service</param>
        /// <param name="transactionHistory"></param>
        public LP48SendLastAcceptedBillInformationHandler(IMeterManager meterManager, ITransactionHistory transactionHistory)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendLastBillInformation
        };

        /// <inheritdoc />
        public SendLastAcceptedBillInformationResponse Handle(LongPollData data)
        {
            var lastTransaction = _transactionHistory.RecallTransactions<BillTransaction>()
                .OrderByDescending(t => t.TransactionId).FirstOrDefault(x => x.State == CurrencyState.Accepted);
            if (lastTransaction is null ||
                !Enum.TryParse<ISOCurrencyCode>(lastTransaction.CurrencyId, out var currencyId))
            {
                return new SendLastAcceptedBillInformationResponse();
            }

            var countryCode = SASCountryCodes.ToSASCountryCode(currencyId);
            // SasProperties.SasLastBillAcceptedKey property is set in bill acceptor note value (dollars),
            // but need billValue to be in cents when getting denom code and meter name

            var billValue = lastTransaction.Amount.MillicentsToCents();
            var denomCode = Utilities.ConvertBillValueToDenominationCode(billValue);
            var billCount = GetMeter(billValue);
            return billCount < 0 || denomCode < 0
                ? new SendLastAcceptedBillInformationResponse()
                : new SendLastAcceptedBillInformationResponse(
                    countryCode,
                    (BillDenominationCodes)denomCode,
                    (ulong)billCount);
        }

        private long GetMeter(long billValue)
        {
            var meterName = DenominationToMeterName.ToMeterName(billValue);
            if (!_meterManager.IsMeterProvided(meterName))
            {
                return 0;
            }

            var meter = _meterManager.GetMeter(meterName);
            return meter.Lifetime;
        }
    }
}
