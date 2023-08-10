namespace Aristocrat.Monaco.Accounting.Contracts.CoinAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;

    /// <summary>
    ///     CoinTransaction defines the data necessary to store a coin.
    /// </summary>
    [Serializable]
    public sealed class CoinInTransaction : BaseTransaction, IEquatable<CoinInTransaction>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinInTransaction" /> class.
        /// </summary>
        public CoinInTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinInTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">Value to set as DeviceId.</param>
        /// <param name="transactionDateTime">Value to set as TransactionDateTime.</param>
        public CoinInTransaction(
            int deviceId,
            DateTime transactionDateTime)
            : base(deviceId, transactionDateTime)
        {
            Exception = (int)CurrencyInExceptionCode.None;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinInTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">Value to set as DeviceId.</param>
        /// <param name="transactionDateTime">Value to set as TransactionDateTime.</param>
        /// <param name="details">Value to set as details code.</param>
        /// <param name="exception">An exception code</param>
        public CoinInTransaction(
            int deviceId,
            DateTime transactionDateTime,
            int details,
            int exception)
            : this(deviceId, transactionDateTime)
        {
            Details = details;
            Exception = exception;
        }

        /// <inheritdoc />
        public override string Name =>
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinTransactionName);

        /// <summary>
        ///     Gets the type of account.
        /// </summary>
        public AccountType TypeOfAccount => AccountType.Cashable;

        /// <summary>
        ///     Gets or sets the time that the coin was accepted
        /// </summary>
        public DateTime Accepted { get; set; }

        /// <summary>
        ///     Gets or sets an details code
        /// </summary>
        public int Details { get; set; }

        /// <summary>
        ///     Gets or sets an exception code
        /// </summary>
        public int Exception { get; set; }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="coinTransaction1">The first transaction</param>
        /// <param name="coinTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(CoinInTransaction coinTransaction1, CoinInTransaction coinTransaction2)
        {
            if (ReferenceEquals(coinTransaction1, coinTransaction2))
            {
                return true;
            }

            if (coinTransaction1 is null || coinTransaction2 is null)
            {
                return false;
            }

            return coinTransaction1.Equals(coinTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="coinTransaction1">The first transaction</param>
        /// <param name="coinTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(CoinInTransaction coinTransaction1, CoinInTransaction coinTransaction2)
        {
            return !(coinTransaction1 == coinTransaction2);
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }
            Accepted = (DateTime)values["Accepted"];
            Details = (int)values["Details"];
            Exception = (int)values["Exception"];

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);
            using (var transaction = block.StartTransaction())
            {
                transaction[element, "Accepted"] = Accepted;
                transaction[element, "Details"] = Details;
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
                "{0} [DeviceId={1}, LogSequence={2}, DateTime={3}, TransactionId={4}, TypeOfAccount={5}, Details={6}, Exception={7}]",
                GetType(),
                DeviceId,
                LogSequence,
                TransactionDateTime.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                TypeOfAccount,
                CoinAccountingExtensions.GetDetailsMessage(Details),
                CurrencyAccountingExtensions.GetDetailsMessage(CurrencyState.Accepted, Exception));

            return builder.ToString();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return new CoinInTransaction(DeviceId, TransactionDateTime, Details, Exception)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                Accepted = Accepted,
                Details = Details,
                Exception = Exception
            };
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as CoinInTransaction);
        }

        /// <summary>
        ///     Checks that two CoinTransaction are the same by value.
        /// </summary>
        /// <param name="other">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(CoinInTransaction other)
        {
            return base.Equals(other);
        }
    }
}
