namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;

    /// <summary>
    ///     KeyedCreditsTransaction defines the data required to log keyed on/off credits in the transaction history
    /// </summary>
    [Serializable]
    public class KeyedCreditsTransaction : BaseTransaction
    {
        private bool _keyedOn;

        /// <summary>
        /// the Keyed On bool
        /// </summary>
        public bool KeyedOn
        {
            get => _keyedOn;
            private set => _keyedOn = value;
        }
        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyedCreditsTransaction" /> class.
        /// </summary>
        public KeyedCreditsTransaction()
        {
            // Need to support for loading from persistence
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
        ///     Initializes a new instance of the <see cref="KeyedCreditsTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">Value to set as DeviceId.</param>
        /// <param name="transactionDateTime">Value to set as TransactionDateTime.</param>
        /// <param name="accountType">Value to set as AccountType.</param>
        /// <param name="keyedOn">Value to set as KeyedOn.</param>
        /// <param name="amount">Value to set as Amount.</param>
        public KeyedCreditsTransaction(
            int deviceId,
            DateTime transactionDateTime,
            AccountType accountType,
            bool keyedOn,
            long amount
        )
            : base(deviceId, transactionDateTime)
        {
            AccountType = accountType;
            _keyedOn = keyedOn;
            Amount = amount;
        }

        /// <summary>
        ///     Gets the account type being keyed on/off
        /// </summary>
        public AccountType AccountType { get; set; }

        /// <summary>
        ///     Gets the credit amount being keyed on/off
        /// </summary>
        public long Amount { get; private set; }

        /// <summary>
        ///     Gets the keyed type - either Keyed On or Keyed Off
        /// </summary>
        public string KeyedType => _keyedOn
            ? Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.KeyedOn)
            : Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.KeyedOff);

        /// <summary>
        ///     Gets the formatted value of credits that are keyed on/off for displaying in UI
        /// </summary>
        public string FormattedValue => Amount.MillicentsToDollars().FormattedCurrencyString();

        /// <inheritdoc />
        public override string Name => _keyedOn
            ? Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.KeyedOnCredits)
            : Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.KeyedOffCredits);

        /// <inheritdoc />
        public override object Clone()
        {
            return new KeyedCreditsTransaction(DeviceId, TransactionDateTime, AccountType, _keyedOn, Amount)
            {
                LogSequence = LogSequence, TransactionId = TransactionId
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
            Amount = (long)values["Amount"];
            _keyedOn = (bool)values["KeyedOn"];

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);
            using (var transaction = block.StartTransaction())
            {
                transaction[element, "AccountType"] = (byte)AccountType;
                transaction[element, "Amount"] = Amount;
                transaction[element, "KeyedOn"] = _keyedOn;
                transaction.Commit();
            }
        }
    }
}