namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;
    using Transactions;

    /// <summary>
    ///     ResidualCreditsTransaction encapsulates and persists the data for a single
    ///     hard meter reset transaction.
    /// </summary>
    public class ResidualCreditsTransaction : BaseTransaction
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ResidualCreditsTransaction" /> class.
        ///     This constructor is only used by the transaction framework.
        /// </summary>
        public ResidualCreditsTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResidualCreditsTransaction" /> class.
        /// </summary>
        /// <param name="transactionId">bank transaction id</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="amount">The amount requiring</param>
        public ResidualCreditsTransaction(
            Guid transactionId,
            DateTime transactionDateTime,
            long amount
            )
            : base(0, transactionDateTime)
        {
            BankTransactionId = transactionId;
            Amount = amount;
        }

        /// <summary>
        ///     Gets the amount of money involved in the transaction.
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Gets or sets the associated bank transaction Id
        /// </summary>
        public Guid BankTransactionId { get; set; }

        /// <inheritdoc />
        public override string Name => Localizer.For(CultureFor.Player).GetString(ResourceKeys.ResidualCreditsTransactionName);

        /// <inheritdoc />
        public long TransactionAmount => Amount;

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            var bankTransId = values["BankTransactionId"];
            if (bankTransId != null)
            {
                BankTransactionId = (Guid)bankTransId;
            }

            Amount = (long)values["Amount"];

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            using var transaction = block.StartTransaction();

            base.SetPersistence(block, element);

            transaction[element, "BankTransactionId"] = BankTransactionId;
            transaction[element, "Amount"] = Amount;
            transaction.Commit();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} LogSequence={1}, DateTime={2}, TransactionId={3}, Amount={4}]",
                GetType(),
                LogSequence,
                TransactionDateTime.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                Amount
                );

            return builder.ToString();
        }

        /// <inheritdoc />
        public override object Clone()
        {
            var copy = new ResidualCreditsTransaction(
                BankTransactionId,
                TransactionDateTime,
                Amount)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
            };

            return copy;
        }
    }
}
