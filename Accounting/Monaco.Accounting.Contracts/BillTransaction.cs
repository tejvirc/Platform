namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;

    /// <summary>
    ///     BillTransaction defines the data necessary to store a bill in NVRam for recall purposes.
    /// </summary>
    [Serializable]
    public class BillTransaction : BaseTransaction
    {
        private const int CurrencyIdLength = 3;

        private char[] _currencyId = new char[CurrencyIdLength];

        /// <summary>
        ///     Initializes a new instance of the <see cref="BillTransaction" /> class.
        /// </summary>
        public BillTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BillTransaction" /> class.
        /// </summary>
        /// <param name="currencyId">Value to set as _currencyId.</param>
        /// <param name="deviceId">Value to set as DeviceId.</param>
        /// <param name="transactionDateTime">Value to set as TransactionDateTime.</param>
        /// <param name="amount">The currency amount of the bill</param>
        public BillTransaction(
            IReadOnlyList<char> currencyId,
            int deviceId,
            DateTime transactionDateTime,
            long amount)
            : base(deviceId, transactionDateTime)
        {
            for (var i = 0; i < CurrencyIdLength; i++)
            {
                _currencyId[i] = currencyId[i];
            }

            Amount = amount;
            State = CurrencyState.Pending;
            Exception = (int)CurrencyInExceptionCode.None;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BillTransaction" /> class.
        /// </summary>
        /// <param name="currencyId">Value to set as _currencyId.</param>
        /// <param name="deviceId">Value to set as DeviceId.</param>
        /// <param name="transactionDateTime">Value to set as TransactionDateTime.</param>
        /// <param name="amount">The currency amount of the bill</param>
        /// <param name="state">The state of the bill acceptance</param>
        /// <param name="exception">An exception code</param>
        public BillTransaction(
            IReadOnlyList<char> currencyId,
            int deviceId,
            DateTime transactionDateTime,
            long amount,
            CurrencyState state,
            int exception)
            : this(currencyId, deviceId, transactionDateTime, amount)
        {
            State = state;
            Exception = exception;
        }

        /// <inheritdoc />
        public override string Name =>
            Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.BillTransactionName);

        /// <summary>
        ///     Gets the currency of the note. Currency identifier; must conform to ISO-4217
        ///     3-character alphabetic standard.
        /// </summary>
        public string CurrencyId => new string(_currencyId);

        /// <summary>
        ///     Gets the denomination of the note inserted
        /// </summary>
        public long Denomination { get; set; }

        /// <summary>
        ///     Gets the amount of money involved in the transaction.
        /// </summary>
        public long Amount { get; private set; }

        /// <summary>
        ///     Gets the type of account.
        /// </summary>
        public AccountType TypeOfAccount => AccountType.Cashable;

        /// <summary>
        ///     Gets or sets the time that the bill was accepted
        /// </summary>
        public DateTime Accepted { get; set; }

        /// <summary>
        ///     Gets or sets the current note state
        /// </summary>
        public CurrencyState State { get; set; }

        /// <summary>
        ///     Gets or sets an exception code
        /// </summary>
        public int Exception { get; set; }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="billTransaction1">The first transaction</param>
        /// <param name="billTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(BillTransaction billTransaction1, BillTransaction billTransaction2)
        {
            if (ReferenceEquals(billTransaction1, billTransaction2))
            {
                return true;
            }

            if (billTransaction1 is null || billTransaction2 is null)
            {
                return false;
            }

            return billTransaction1.Equals(billTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="billTransaction1">The first transaction</param>
        /// <param name="billTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(BillTransaction billTransaction1, BillTransaction billTransaction2)
        {
            return !(billTransaction1 == billTransaction2);
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            _currencyId = Encoding.ASCII.GetChars((byte[])values["CurrencyId"]);
            Amount = (long)values["Amount"];
            Denomination = (long)values["Denomination"];
            Accepted = (DateTime)values["Accepted"];
            State = (CurrencyState)values["State"];
            Exception = (int)values["Exception"];

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);
            using (var transaction = block.StartTransaction())
            {
                transaction[element, "CurrencyId"] = Encoding.ASCII.GetBytes(_currencyId);
                transaction[element, "Amount"] = Amount;
                transaction[element, "Denomination"] = Denomination;
                transaction[element, "Accepted"] = Accepted;
                transaction[element, "State"] = State;
                transaction[element, "Exception"] = Exception;
                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} [DeviceId={1}, LogSequence={2}, DateTime={3}, TransactionId={4}, Amount={5}, TypeOfAccount={6}, CurrencyId={7}, State={8}, Exception={9}]",
                GetType(),
                DeviceId,
                LogSequence,
                TransactionDateTime.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                Amount,
                TypeOfAccount,
                CurrencyId,
                CurrencyAccountingExtensions.GetStatusText(State),
                CurrencyAccountingExtensions.GetDetailsMessage(State, Exception));

            return builder.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as BillTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ CurrencyId.GetHashCode();
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return new BillTransaction(_currencyId, DeviceId, TransactionDateTime, Amount, State, Exception)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                Accepted = Accepted,
                Denomination = Denomination,
                State = State,
                Exception = Exception
            };
        }

        /// <summary>
        ///     Checks that two BillTransaction are the same by value.
        /// </summary>
        /// <param name="billTransaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(BillTransaction billTransaction)
        {
            return base.Equals(billTransaction) && CurrencyId == billTransaction.CurrencyId;
        }
    }
}