namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    public class KeyedOffCreditsTransaction : BaseTransaction
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyedOffCreditsTransaction" /> class.
        /// </summary>
        public KeyedOffCreditsTransaction()
        {
            // Need to support for loading from persistence
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyedOffCreditsTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">Value to set as DeviceId.</param>
        /// <param name="transactionDateTime">Value to set as TransactionDateTime.</param>
        /// <param name="accountType">Value to set as AccountType.</param>
        /// <param name="transferredCashableAmount">todo</param>
        /// <param name="transferredPromoAmount">todo</param>
        /// <param name="transferredNonCashAmount">todo</param>
        public KeyedOffCreditsTransaction(
            int deviceId,
            DateTime transactionDateTime,
            AccountType accountType,
            long transferredCashableAmount,
            long transferredPromoAmount,
            long transferredNonCashAmount
        )
            : base(deviceId, transactionDateTime)
        {
            AccountType = accountType;
            TransferredCashableAmount = transferredCashableAmount;
            TransferredPromoAmount = transferredPromoAmount;
            TransferredNonCashAmount = transferredNonCashAmount;
        }

        /// <summary>
        ///     Gets the transferred cashable amount
        /// </summary>
        public long TransferredCashableAmount { get; set; }

        /// <summary>
        ///     Gets the transferred promo amount
        /// </summary>
        public long TransferredPromoAmount { get; set; }

        /// <summary>
        ///     Gets the transferred non-cashable amount
        /// </summary>
        public long TransferredNonCashAmount { get; set; }

        /// <summary>
        ///     Gets the account type being keyed on/off
        /// </summary>
        public AccountType AccountType { get; set; }

        /// <summary>
        ///     Gets the keyed type - either Keyed On or Keyed Off
        /// </summary>
        public string KeyedType =>  Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.KeyedOff);

        /// <inheritdoc />
        public override string Name => Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.KeyedOffCredits);

        /// <inheritdoc />
        public IEnumerable<long> AssociatedTransactions { get; set; }

        /// <summary>
        ///     Gets the formatted value of credits that are keyed on/off for displaying in UI
        /// </summary>
        public string FormattedValue => TransactionAmount.MillicentsToDollars().FormattedCurrencyString();

        /// <inheritdoc />
        public long TransactionAmount => TransferredCashableAmount + TransferredPromoAmount + TransferredNonCashAmount;

        /// <inheritdoc />
        public override object Clone()
        {
            return new KeyedOffCreditsTransaction(
                DeviceId,
                TransactionDateTime,
                AccountType,
                TransferredCashableAmount,
                TransferredPromoAmount,
                TransferredNonCashAmount)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                TransferredCashableAmount = TransferredCashableAmount,
                TransferredPromoAmount = TransferredPromoAmount,
                TransferredNonCashAmount = TransferredNonCashAmount,
                AccountType = AccountType,
            };
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }
            AccountType = (AccountType)(byte)values["AccountType"];
            TransferredCashableAmount = (long)values["TransferredCashableAmount"];
            TransferredPromoAmount = (long)values["TransferredPromoAmount"];
            TransferredNonCashAmount = (long)values["TransferredNonCashAmount"];
            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);
            using (var transaction = block.StartTransaction())
            {
                transaction[element, "AccountType"] = (byte)AccountType;
                transaction[element, "TransferredCashableAmount"] = TransferredCashableAmount;
                transaction[element, "TransferredPromoAmount"] = TransferredPromoAmount;
                transaction[element, "TransferredNonCashAmount"] = TransferredNonCashAmount;
            }
        }
    }
}
