namespace Aristocrat.Monaco.Gaming.Contracts.Spc
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;

    /// <summary>
    ///     Defines the spc transaction
    /// </summary>
    [Serializable]
    public class SpcTransaction : BaseTransaction, ITransactionConnector
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SpcTransaction"/> class.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="transactionDateTime"></param>
        public SpcTransaction(
            int deviceId,
            DateTime transactionDateTime)
            : base(deviceId, transactionDateTime)
        {
        }

        /// <inheritdoc />
        public override string Name =>
            Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.SpcTransactionName);

        /// <inheritdoc />
        public IEnumerable<long> AssociatedTransactions { get; set; }

        /// <summary>
        ///     Gets the level identifier.
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets the device identifier of the game in which the progressive was hit.
        /// </summary>
        public int GamePlayId { get; set; }

        /// <summary>
        ///     Gets the denomination of the wager.
        /// </summary>
        public long DenomId { get; set; }

        /// <summary>
        ///     Gets the paytable win-level index.
        /// </summary>
        public int WinLevelIndex { get; set; }

        /// <summary>
        ///     Gets the Name that describes the win combination
        ///     associated with the <see cref="WinLevelIndex"/>.
        /// </summary>
        public string WinLevelCombo { get; set; }

        /// <summary>
        ///     Gets the current state of the standalone progressive controller transaction.
        /// </summary>
        public SpcState State { get; set; }

        /// <summary>
        ///     Gets the value of the progressive level at the time of the hit.
        /// </summary>
        public long HitAmount { get; set; }

        /// <summary>
        ///     Gets the amount paid.
        /// </summary>
        public long PaidAmount { get; set; }

        /// <summary>
        ///     Gets the initial startup value included in the hitAmt.
        /// </summary>
        public long StartupAmount { get; set; }

        /// <summary>
        ///     Gets the Value of contributions and/or adjustments not
        ///     yet applied to the progressive level at the time of
        ///     the progressive hit.
        /// </summary>
        public long OverflowAmount { get; set; }

        /// <summary>
        ///     Total of all startup values at the time of the
        ///     progressive hit.
        /// </summary>
        public long TotalStartupAmount { get; set; }

        /// <summary>
        ///     Total of all adjustments made to the progressive
        ///     level at the time of the progressive hit.
        /// </summary>
        public long TotalAdjustAmount { get; set; }

        /// <summary>
        ///     Total of all progressive contributions at the time
        ///     of the progressive hit.
        /// </summary>
        public long TotalContribAmount { get; set; }

        /// <summary>
        ///     Total of all hits for the progressive level; not
        ///     recorded until the level is reset.
        /// </summary>
        public long TotalHitAmount { get; set; }

        /// <summary>
        ///     Total of progressive payments; not recorded until
        ///     the level is reset.
        /// </summary>
        public long TotalPaidAmount { get; set; }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="lhs">The first transaction</param>
        /// <param name="rhs">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(SpcTransaction lhs, SpcTransaction rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            if (lhs is null || rhs is null)
            {
                return false;
            }

            return lhs.Equals(rhs);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="lhs">The first transaction</param>
        /// <param name="rhs">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(SpcTransaction lhs, SpcTransaction rhs)
        {
            return !(lhs == rhs);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as SpcTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (TransactionId, TransactionDateTime).GetHashCode();
        }

        /// <summary>
        ///     Checks that two <see cref="SpcTransaction"/> are the same by value.
        /// </summary>
        /// <param name="transaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(SpcTransaction transaction)
        {
            return base.Equals(transaction) && TransactionId == transaction.TransactionId;
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return new SpcTransaction(DeviceId, TransactionDateTime)
            {
                TransactionId = TransactionId,
                LogSequence = LogSequence,
                LevelId = LevelId,
                GamePlayId = GamePlayId,
                DenomId = DenomId,
                WinLevelIndex = WinLevelIndex,
                WinLevelCombo = WinLevelCombo,
                State = State,
                HitAmount = HitAmount,
                PaidAmount = PaidAmount,
                StartupAmount = StartupAmount,
                OverflowAmount = OverflowAmount,
                TotalStartupAmount = TotalStartupAmount,
                TotalAdjustAmount = TotalAdjustAmount,
                TotalContribAmount = TotalContribAmount,
                TotalHitAmount = TotalHitAmount,
                TotalPaidAmount = TotalPaidAmount
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $@"{typeof(SpcTransaction)} [DeviceId={DeviceId}, LogSequence={LogSequence}, DateTime={TransactionDateTime}, \
                TransactionId={TransactionId}, LogSequence={LogSequence}";
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            var success = base.SetData(values);

            if (success)
            {
                LevelId = (int)values["LevelId"];
                GamePlayId = (int)values["GamePlayId"];
                DenomId = (long)values["DenomId"];
                WinLevelIndex = (int)values["WinLevelIndex"];
                WinLevelCombo = (string)values["WinLevelCombo"];
                State = (SpcState)values["State"];
                HitAmount = (long)values["HitAmount"];
                PaidAmount = (long)values["PaidAmount"];
                StartupAmount = (long)values["StartupAmount"];
                OverflowAmount = (long)values["OverflowAmount"];
                TotalStartupAmount = (long)values["TotalStartupAmount"];
                TotalAdjustAmount = (long)values["TotalAdjustAmount"];
                TotalContribAmount = (long)values["TotalContribAmount"];
                TotalHitAmount = (long)values["TotalHitAmount"];
                TotalPaidAmount = (long)values["TotalPaidAmount"];
            }

            return success;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);

            using (var transaction = block.StartTransaction())
            {
                transaction[element, "LevelId"] = LevelId;
                transaction[element, "GamePlayId"] = GamePlayId;
                transaction[element, "DenomId"] = DenomId;
                transaction[element, "WinLevelIndex"] = WinLevelIndex;
                transaction[element, "WinLevelCombo"] = WinLevelCombo;
                transaction[element, "State"] = State;
                transaction[element, "HitAmount"] = HitAmount;
                transaction[element, "PaidAmount"] = PaidAmount;
                transaction[element, "StartupAmount"] = StartupAmount;
                transaction[element, "OverflowAmount"] = OverflowAmount;
                transaction[element, "TotalStartupAmount"] = TotalStartupAmount;
                transaction[element, "TotalAdjustAmount"] = TotalAdjustAmount;
                transaction[element, "TotalContribAmount"] = TotalContribAmount;
                transaction[element, "TotalHitAmount"] = TotalHitAmount;
                transaction[element, "TotalPaidAmount"] = TotalPaidAmount;
            }
        }
    }
}
