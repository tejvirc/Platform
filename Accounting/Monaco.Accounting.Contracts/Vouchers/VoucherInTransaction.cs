namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;

    /// <summary>
    ///     VoucherInTransaction encapsulates and persists the data for a single
    ///     voucher-in transaction.
    /// </summary>
    [Serializable]
    public class VoucherInTransaction : VoucherBaseTransaction
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherInTransaction" /> class.
        ///     This constructor is only used by the transaction framework.
        /// </summary>
        public VoucherInTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherInTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="barcode">The barcode of the issued voucher</param>
        public VoucherInTransaction(
            int deviceId,
            DateTime transactionDateTime,
            string barcode)
            : base(deviceId, transactionDateTime, 0, AccountType.Cashable, barcode)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherInTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="amount">The currency amount of the voucher</param>
        /// <param name="accountType">The type of credits on the voucher</param>
        /// <param name="barcode">The barcode of the issued voucher</param>
        public VoucherInTransaction(
            int deviceId,
            DateTime transactionDateTime,
            long amount,
            AccountType accountType,
            string barcode)
            : base(deviceId, transactionDateTime, amount, accountType, barcode)
        {
        }

        /// <inheritdoc />
        public override string Name => Localizer.For(CultureFor.Player).GetString(ResourceKeys.VoucherIn);

        /// <summary>
        ///     Gets the voucher sequence for the voucher (specific to Voucher IN transactions)
        /// </summary>
        public int VoucherSequence { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the CommitAcknowledged for the voucher.
        /// </summary>
        public bool CommitAcknowledged { get; set; }

        /// <summary>
        ///     Gets or sets the current voucher state
        /// </summary>
        public VoucherState State { get; set; }

        /// <summary>
        ///     Gets or set the error code for the EGM
        /// </summary>
        public int Exception { get; set; }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="voucherInTransaction1">The first transaction</param>
        /// <param name="voucherInTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(
            VoucherInTransaction voucherInTransaction1,
            VoucherInTransaction voucherInTransaction2)
        {
            if (ReferenceEquals(voucherInTransaction1, voucherInTransaction2))
            {
                return true;
            }

            if (voucherInTransaction1 is null || voucherInTransaction2 is null)
            {
                return false;
            }

            return voucherInTransaction1.Equals(voucherInTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="voucherInTransaction1">The first transaction</param>
        /// <param name="voucherInTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(
            VoucherInTransaction voucherInTransaction1,
            VoucherInTransaction voucherInTransaction2)
        {
            return !(voucherInTransaction1 == voucherInTransaction2);
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            CommitAcknowledged = (bool)values["CommitAcknowledged"];
            VoucherSequence = (int)values["VoucherSequence"];
            State = (VoucherState)values["State"];
            Exception = (int)values["Exception"];

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);
            using (var transaction = block.StartTransaction())
            {
                transaction[element, "VoucherSequence"] = VoucherSequence;
                transaction[element, "CommitAcknowledged"] = CommitAcknowledged;
                transaction[element, "State"] = State;
                transaction[element, "Exception"] = Exception;
                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [DeviceId={1}, LogSequence={2}, DateTime={3}, TransactionId={4}, Amount={5}, TypeOfAccount={6}, Barcode={7}, CommitAcknowledged={8}, VoucherSequence={9}]",
                GetType(),
                DeviceId,
                LogSequence,
                TransactionDateTime.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                Amount,
                TypeOfAccount,
                Barcode,
                CommitAcknowledged,
                VoucherSequence);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var voucherInTransaction = obj as VoucherInTransaction;
            return voucherInTransaction != null && Equals(voucherInTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Barcode.GetHashCode();
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return new VoucherInTransaction(
                DeviceId,
                TransactionDateTime,
                Amount,
                TypeOfAccount,
                Barcode)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                CommitAcknowledged = CommitAcknowledged,
                VoucherSequence = VoucherSequence,
                State = State,
                Exception = Exception,
                LogDisplayType = LogDisplayType
            };
        }

        /// <summary>
        ///     Checks that two VoucherInTransaction are the same by value.
        /// </summary>
        /// <param name="voucherInTransaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(VoucherInTransaction voucherInTransaction)
        {
            return voucherInTransaction != null &&
                   Barcode == voucherInTransaction.Barcode &&
                   base.Equals(voucherInTransaction);
        }
    }
}