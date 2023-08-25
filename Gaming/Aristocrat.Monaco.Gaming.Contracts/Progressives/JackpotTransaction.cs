namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;

    /// <summary>
    ///     JackpotTransaction encapsulates and persists the data for a single Jackpot transaction.
    /// </summary>
    [Serializable]
    public class JackpotTransaction : BaseTransaction, IViewableJackpotTransaction
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JackpotTransaction" /> class.
        /// </summary>
        /// <remarks>
        ///     This constructor is only used by the transaction framework.
        /// </remarks>
        public JackpotTransaction()
        {
        }

        ///  <summary>
        ///      Initializes a new instance of the <see cref="JackpotTransaction" /> class.
        ///  </summary>
        ///  <param name="deviceId">The transaction device identifier.</param>
        ///  <param name="dateTime">The date and time the progressive hit occurred.</param>
        ///  <param name="progId">The progressive identifier.</param>
        ///  <param name="levelId">The level identifier.</param>
        ///  <param name="gameId">The game identifier.</param>
        ///  <param name="denomId">The denomination identifier.</param>
        ///  <param name="winLevelIndex">The paytable win index.</param>
        ///  <param name="amount">
        ///      The Value of the progressive; set by the EGM to the last known prize value when the progressive hit occurred; set
        ///      to 0 (zero) if the prize value is unknown.
        ///  </param>
        ///  <param name="valueText">
        ///      Gets or sets the text representation of the progressive value; set by the EGM to the last known text value when
        ///      the progressive hit occurred.
        ///  </param>
        ///  <param name="valueSequence">Used to sequence progressive updates.</param>
        ///  <param name="resetValue">The reset value.</param>
        ///  <param name="payMethod">Method of payment for the award.</param>
        /// <param name="assignableProgressiveType">Type of Progressive.</param>
        ///  <param name="assignedProgressiveKey">Progressive Key"</param>
        ///  <param name="hiddenTotal">Progressive level hidden total</param>
        ///  <param name="overflow">Progressive level overflow amount</param>
        ///  <param name="bulkTotal">Progressive level bulk total</param>
        public JackpotTransaction(
            int deviceId,
            DateTime dateTime,
            int progId,
            int levelId,
            int gameId,
            long denomId,
            int winLevelIndex,
            long amount,
            string valueText,
            long valueSequence,
            long resetValue,
            int assignableProgressiveType,
            string assignedProgressiveKey,
            PayMethod payMethod,
            long hiddenTotal,
            long overflow,
            long bulkTotal)
            : base(deviceId, dateTime)
        {
            ProgressiveId = progId;
            LevelId = levelId;
            GameId = gameId;
            DenomId = denomId;
            WinLevelIndex = winLevelIndex;
            ValueAmount = amount;
            ValueText = valueText;
            ValueSequence = valueSequence;
            ResetValue = resetValue;
            AssignableProgressiveType = assignableProgressiveType;
            AssignedProgressiveKey = assignedProgressiveKey;
            PayMethod = payMethod;

            State = ProgressiveState.Hit;
            PaidAmount = 0;
            Exception = 0;

            HiddenTotal = hiddenTotal;
            Overflow = overflow;
            BulkTotal = bulkTotal;
        }

        /// <summary>
        ///     Gets the human readable name for this transaction type
        /// </summary>
        public override string Name =>
            Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.JackpotTransactionName);

        /// <summary>
        ///     Gets or sets the progressive Id
        /// </summary>
        public int ProgressiveId { get; set; }

        /// <summary>
        ///     Gets or sets the level identifier
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets or sets the current state of the progressive
        /// </summary>
        public ProgressiveState State { get; set; }

        /// <summary>
        ///     Gets or sets the game identifier
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        ///     Gets or sets the denomination
        /// </summary>
        public long DenomId { get; set; }

        /// <summary>
        ///     Gets or sets the paytable win level index
        /// </summary>
        public int WinLevelIndex { get; set; }

        /// <summary>
        ///     Gets or sets the reset value.
        /// </summary>
        /// <value>The reset value.</value>
        public long ResetValue { get; set; }

        /// <summary>
        ///     Gets or sets the Progressive Type
        /// </summary>
        /// <value>The reset value.</value>
        public int AssignableProgressiveType { get; set; }

        /// <summary>
        ///     Gets or sets the Progressive Key.
        /// </summary>
        /// <value>The reset value.</value>
        public string AssignedProgressiveKey { get; set; }

        /// <summary>
        ///     Gets the progressive value amount
        /// </summary>
        public long ValueAmount { get; set; }

        /// <summary>
        ///     Gets or sets the text representation of the progressive value; set by the EGM to the last known text value when
        ///     the progressive hit occurred
        /// </summary>
        public string ValueText { get; set; }

        /// <summary>
        ///     Gets or sets a strictly increasing series; set by the EGM to the last known sequence value when the progressive hit
        ///     occurred
        /// </summary>
        public long ValueSequence { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the bonus.
        /// </summary>
        /// <value>The identifier of the bonus.</value>
        public string BonusId { get; set; }

        /// <summary>
        ///     Gets or sets the value of the prize awarded to the EGM when the progressive hit was processed; due to delays
        ///     sending
        ///     updates to the EGM or simultaneous wins, this value may be different than the value reported by the EGM in the
        ///     progressiveHit command.
        /// </summary>
        public long WinAmount { get; set; }

        /// <summary>
        ///     Gets or sets the text representation of the progressive value; set by the host to the text value for
        ///     the prize awarded to the EGM when the progressive hit was processed
        /// </summary>
        public string WinText { get; set; }

        /// <summary>
        ///     Gets or sets a strictly increasing series; set by the host to the sequence value for the prize awarded to the EGM
        ///     when the progressive hit was processed; due to delays sending updates to the EGM or simultaneous wins, this value may be
        ///     different than the value reported by the EGM in the progressiveHit command.
        /// </summary>
        /// <value>The window sequence.</value>
        public long WinSequence { get; set; }

        /// <summary>
        ///     Gets or sets the method of payment for the award.
        /// </summary>
        /// <value>The pay method.</value>
        public PayMethod PayMethod { get; set; }

        /// <summary>
        ///     Gets or sets the value of the progressive payment; set by the EGM to the value of the prize actually paid to the
        ///     player; due to rounding, this value may be different than the value specified by the host
        /// </summary>
        public long PaidAmount { get; set; }

        /// <summary>
        ///     Gets or sets the progressive Exception Code
        /// </summary>
        public byte Exception { get; set; }

        /// <summary>
        ///     Gets or sets the Date/time that the progressive win was awarded
        /// </summary>
        public DateTime PaidDateTime { get; set; }

        /// <summary>
        ///     The value of the hidden pool (in millicents) at the time of jackpot
        /// </summary>
        public long HiddenTotal { get; set; }

        /// <summary>
        ///     The overflow amount at the time of jackpot
        /// </summary>
        public long Overflow { get; set; }

        /// <summary>
        ///     The value of the bulk pool (in millicents) at the time of jackpot
        /// </summary>
        public long BulkTotal { get; set; }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="jackpotTransaction1">The first transaction</param>
        /// <param name="jackpotTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(JackpotTransaction jackpotTransaction1, JackpotTransaction jackpotTransaction2)
        {
            if (ReferenceEquals(jackpotTransaction1, jackpotTransaction2))
            {
                return true;
            }

            if (jackpotTransaction1 is null || jackpotTransaction2 is null)
            {
                return false;
            }

            return jackpotTransaction1.Equals(jackpotTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="jackpotTransaction1">The first transaction</param>
        /// <param name="jackpotTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(JackpotTransaction jackpotTransaction1, JackpotTransaction jackpotTransaction2)
        {
            return !(jackpotTransaction1 == jackpotTransaction2);
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            State = (ProgressiveState)(short)values["State"];
            ProgressiveId = (int)values["ProgressiveId"];
            LevelId = (int)values["LevelId"];
            GameId = (int)values["GameId"];
            DenomId = (long)values["DenomId"];
            ResetValue = (long)values["ResetValue"];
            WinLevelIndex = (int)values["WinLevelIndex"];
            ValueText = (string)values["ValueText"];
            ValueSequence = (long)values["ValueSequence"];
            ValueAmount = (long)values["Amount"];
            WinAmount = (long)values["WinAmount"];
            WinText = (string)values["WinText"];
            WinSequence = (long)values["WinSequence"];
            PayMethod = (PayMethod)(byte)values["PayMethod"];
            PaidAmount = (long)values["PaidAmount"];
            Exception = (byte)values["Exception"];
            PaidDateTime = (DateTime)values["PaidDateTime"];
            BonusId = (string)values["BonusId"];
            AssignedProgressiveKey = (string)values["AssignedProgressiveKey"];
            AssignableProgressiveType = (int)values["AssignableProgressiveType"];
            HiddenTotal = (long)values["HiddenValue"];
            Overflow = (long)values["Overflow"];
            BulkTotal = (long)values["BulkValue"];

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);

            using (var transaction = block.StartTransaction())
            {
                transaction[element, "State"] = (short)State;
                transaction[element, "ProgressiveId"] = ProgressiveId;
                transaction[element, "LevelId"] = LevelId;
                transaction[element, "GameId"] = GameId;
                transaction[element, "DenomId"] = DenomId;
                transaction[element, "ResetValue"] = ResetValue;
                transaction[element, "WinLevelIndex"] = WinLevelIndex;
                transaction[element, "Amount"] = ValueAmount;
                transaction[element, "ValueText"] = ValueText;
                transaction[element, "ValueSequence"] = ValueSequence;
                transaction[element, "WinAmount"] = WinAmount;
                transaction[element, "WinText"] = WinText;
                transaction[element, "WinSequence"] = WinSequence;
                transaction[element, "PayMethod"] = (byte)PayMethod;
                transaction[element, "PaidAmount"] = PaidAmount;
                transaction[element, "Exception"] = Exception;
                transaction[element, "PaidDateTime"] = PaidDateTime;
                transaction[element, "BonusId"] = BonusId;
                transaction[element, "AssignedProgressiveKey"] = AssignedProgressiveKey;
                transaction[element, "AssignableProgressiveType"] = AssignableProgressiveType;
                transaction[element, "HiddenValue"] = HiddenTotal;
                transaction[element, "Overflow"] = Overflow;
                transaction[element, "BulkValue"] = BulkTotal;

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{GetType()} [DeviceId={DeviceId}, TransactionId={TransactionId}, LogSequence={LogSequence}, DateTime={TransactionDateTime}, TransactionId={TransactionId}, ValueAmount={ValueAmount}, WinAmount={WinAmount}, PaidAmount={PaidAmount}, State={State}]";
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as JackpotTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return new
            {
                TransactionId,
                DeviceId,
                ProgId = ProgressiveId,
                LevelId,
                GameId,
                DenomId,
                ValueAmount,
                WinAmount,
                PaidAmount,
                State
            }.GetHashCode();
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        ///     Checks that two JackpotTransactions are the same by value.
        /// </summary>
        /// <param name="jackpotTransaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(JackpotTransaction jackpotTransaction)
        {
            return base.Equals(jackpotTransaction) &&
                   ProgressiveId == jackpotTransaction.ProgressiveId &&
                   LevelId == jackpotTransaction.LevelId &&
                   GameId == jackpotTransaction.GameId &&
                   DenomId == jackpotTransaction.DenomId &&
                   WinLevelIndex == jackpotTransaction.WinLevelIndex &&
                   PaidAmount == jackpotTransaction.PaidAmount &&
                   WinAmount == jackpotTransaction.WinAmount &&
                   WinText == jackpotTransaction.WinText &&
                   WinSequence == jackpotTransaction.WinSequence &&
                   ValueAmount == jackpotTransaction.ValueAmount &&
                   ValueText == jackpotTransaction.ValueText &&
                   ValueSequence == jackpotTransaction.ValueSequence &&
                   BonusId == jackpotTransaction.BonusId &&
                   PayMethod == jackpotTransaction.PayMethod &&
                   Exception == jackpotTransaction.Exception &&
                   State == jackpotTransaction.State &&
                   AssignedProgressiveKey == jackpotTransaction.AssignedProgressiveKey &&
                   AssignableProgressiveType == jackpotTransaction.AssignableProgressiveType &&
                   HiddenTotal == jackpotTransaction.HiddenTotal &&
                   Overflow == jackpotTransaction.Overflow &&
                   BulkTotal == jackpotTransaction.BulkTotal;
        }
    }
}