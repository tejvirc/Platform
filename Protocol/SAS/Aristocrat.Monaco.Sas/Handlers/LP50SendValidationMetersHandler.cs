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
    using Aristocrat.Sas.Client.Metering;
    using Contracts.SASProperties;
    using Kernel;

    /// <inheritdoc />
    public class LP50SendValidationMetersHandler :
        ISasLongPollHandler<SendValidationMetersResponse, LongPollSingleValueData<TicketValidationType>>
    {
        private const long DefaultMeterValue = 0L;

        private static readonly IList<TicketValidationType> SecureEnhancedMeters = new List<TicketValidationType>
        {
            TicketValidationType.HandPayFromCashOutReceiptPrinted,
            TicketValidationType.HandPayFromWinNoReceipt,
            TicketValidationType.HandPayFromCashOutNoReceipt,
            TicketValidationType.HandPayFromWinReceiptPrinted
        };

        private static readonly IDictionary<TicketValidationType, (IList<string> countMeters, IList<string> amountMeters)> ValidationMeters =
            new Dictionary<TicketValidationType, (IList<string>, IList<string>)>
            {
                {
                    TicketValidationType.CashableTicketFromCashOutOrWin,
                    (new List<string>
                        {
                            AccountingMeters.VoucherOutCashableCount,
                            AccountingMeters.VoucherOutCashablePromoCount
                        },
                        new List<string>
                        {
                            AccountingMeters.VoucherOutCashableAmount,
                            AccountingMeters.VoucherOutCashablePromoAmount
                        })
                },
                {
                    TicketValidationType.RestrictedPromotionalTicketFromCashOut,
                    (new List<string> { AccountingMeters.VoucherOutNonCashableCount },
                        new List<string> { AccountingMeters.VoucherOutNonCashableAmount })
                },
                {
                    TicketValidationType.CashableTicketRedeemed,
                    (new List<string> { AccountingMeters.VoucherInCashableCount },
                        new List<string> { AccountingMeters.VoucherInCashableAmount })
                },
                {
                    TicketValidationType.RestrictedPromotionalTicketRedeemed,
                    (new List<string> { AccountingMeters.VoucherInNonCashableCount },
                        new List<string> { AccountingMeters.VoucherInNonCashableAmount })
                },
                {
                    TicketValidationType.NonRestrictedPromotionalTicketRedeemed,
                    (new List<string> { AccountingMeters.VoucherInCashablePromoCount },
                        new List<string> { AccountingMeters.VoucherInCashablePromoAmount })
                },
                {
                    TicketValidationType.HandPayFromCashOutReceiptPrinted,
                    (new List<string> { AccountingMeters.HandpaidValidatedCancelReceiptCount },
                        new List<string> { AccountingMeters.HandpaidValidatedCancelReceiptAmount })
                },
                {
                    TicketValidationType.HandPayFromWinReceiptPrinted,
                    (new List<string> { SasMeterCollection.SasMeterForCode(SasMeterId.ValidatedJackpotHandPayReceiptCount).MappedMeterName },
                        new List<string> { SasMeterCollection.SasMeterForCode(SasMeterId.ValidatedJackpotHandPayReceiptCents).MappedMeterName })
                },
                {
                    TicketValidationType.HandPayFromCashOutNoReceipt,
                    (new List<string> { AccountingMeters.HandpaidValidatedCancelNoReceiptCount },
                        new List<string> { AccountingMeters.HandpaidValidatedCancelNoReceiptAmount })
                },
                {
                    TicketValidationType.HandPayFromWinNoReceipt,
                    (new List<string> { SasMeterCollection.SasMeterForCode(SasMeterId.ValidatedJackpotHandPayNoReceiptCount).MappedMeterName },
                        new List<string> { SasMeterCollection.SasMeterForCode(SasMeterId.ValidatedJackpotHandPayNoReceiptCents).MappedMeterName })
                },
                // Unsupported transaction type so we just need return 0
                {
                    TicketValidationType.CashableTicketFromAftTransfer,
                    (new List<string>(), new List<string>())
                },
                {
                    TicketValidationType.RestrictedTicketFromAftTransfer,
                    (new List<string>(), new List<string>())
                },
                {
                    TicketValidationType.DebitTicketFromAftTransfer,
                    (new List<string>(), new List<string>())
                }
            };

        private readonly IMeterManager _meterManager;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Creates the LP50SendValidationMetersHandler instance
        /// </summary>
        /// <param name="meterManager">The meter manager</param>
        /// <param name="propertiesManager">The properties manager</param>
        public LP50SendValidationMetersHandler(IMeterManager meterManager, IPropertiesManager propertiesManager)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendValidationMeters
        };

        /// <inheritdoc />
        public SendValidationMetersResponse Handle(LongPollSingleValueData<TicketValidationType> data)
        {
            var validationType = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .ValidationType;
            if (!ValidationMeters.TryGetValue(data.Value, out var meters) ||
                meters.countMeters.Count == 0 || meters.amountMeters.Count == 0 ||
                // Hand pay Meters are only for secure enhanced as we should not validate any hand pay when not using secure enhanced
                (validationType != SasValidationType.SecureEnhanced && SecureEnhancedMeters.Contains(data.Value)))
            {
                return new SendValidationMetersResponse(DefaultMeterValue, DefaultMeterValue);
            }

            var countTotal = meters.countMeters.Aggregate(
                (long)0,
                (current, countMeter) => current + GetMeter(countMeter));
            var amountTotal = meters.amountMeters.Aggregate(
                (long)0,
                (current, amountMeter) => current + GetMeter(amountMeter));

            return new SendValidationMetersResponse(countTotal, amountTotal.MillicentsToCents());
        }

        private long GetMeter(string meterName)
        {
            return _meterManager.IsMeterProvided(meterName) ? _meterManager.GetMeter(meterName).Lifetime : DefaultMeterValue;
        }
    }
}