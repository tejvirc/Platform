namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;
    using Newtonsoft.Json;

    /// <summary>
    ///     Defines the bonus transaction
    /// </summary>
    [Serializable]
    public class BonusTransaction : BaseTransaction, ITransactionConnector
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusTransaction" /> class.
        /// </summary>
        /// <remarks>
        ///     This constructor is only used by the transaction framework.
        /// </remarks>
        public BonusTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="dateTime">The date and time the progressive hit occurred</param>
        /// <param name="bonusId">The bonus Id</param>
        /// <param name="cashableAmount">The award cashable amount</param>
        /// <param name="nonCashAmount">The award non cash amount</param>
        /// <param name="promoAmount">The award promo amount</param>
        /// <param name="gameId">The associated game identifier</param>
        /// <param name="denom">The associated denomination</param>
        /// <param name="payMethod">The payment method</param>
        public BonusTransaction(
            int deviceId,
            DateTime dateTime,
            string bonusId,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount,
            int gameId,
            long denom,
            PayMethod payMethod)
            : base(deviceId, dateTime)
        {
            LastUpdate = dateTime;
            BonusId = bonusId;

            DisplayMessageId = Guid.NewGuid();
            MessageDuration = TimeSpan.MaxValue;

            CashableAmount = cashableAmount;
            NonCashAmount = nonCashAmount;
            PromoAmount = promoAmount;
            GameId = gameId;
            Denom = denom;
            PayMethod = payMethod;

            State = BonusState.Pending;
            Mode = BonusMode.Standard;

            AssociatedTransactions = Enumerable.Empty<long>();
        }

        /// <inheritdoc />
        public override string Name =>
            Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.BonusTransactionName);

        /// <summary>
        ///     Sets Unique Id assigned when creating the bonus award
        /// </summary>
        public string BonusId { get; private set; }

        /// <summary>
        ///     Gets or sets the bonus state
        /// </summary>
        public BonusState State { get; set; }

        /// <summary>
        ///     Gets the Cashable Amount.
        /// </summary>
        public long CashableAmount { get; private set; }

        /// <summary>
        ///     Gets the Non Cash Amount.
        /// </summary>
        public long NonCashAmount { get; private set; }

        /// <summary>
        ///     Gets the Promo Amount.
        /// </summary>
        public long PromoAmount { get; private set; }

        /// <summary>
        ///     Gets the game identifier
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        ///     Gets the denomination
        /// </summary>
        public long Denom { get; set; }

        /// <summary>
        ///     Gets the pay method
        /// </summary>
        public PayMethod PayMethod { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not an Player Id is required
        /// </summary>
        public bool IdRequired { get; set; }

        /// <summary>
        ///     Gets or set the Id Number that was present when the bonus was awarded
        /// </summary>
        public string IdNumber { get; set; }

        /// <summary>
        ///     Gets or sets the player Id that was present when the bonus was awarded
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        ///     Gets the actual bonus award amount
        /// </summary>
        public long PaidAmount => PaidCashableAmount + PaidNonCashAmount + PaidPromoAmount;

        /// <summary>
        ///     Gets or sets the cashable bonus award amount
        /// </summary>
        public long PaidCashableAmount { get; set; }

        /// <summary>
        ///     Gets or sets the non-cash bonus award amount
        /// </summary>
        public long PaidNonCashAmount { get; set; }

        /// <summary>
        ///     Gets or sets the promo bonus award amount
        /// </summary>
        public long PaidPromoAmount { get; set; }

        /// <summary>
        ///     Gets or sets the paid date/time
        /// </summary>
        public DateTime PaidDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the last update date/time
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        ///     Gets or sets the exception code
        /// </summary>
        public int Exception { get; set; }

        /// <summary>
        ///     Gets or sets the exception code
        /// </summary>
        public int ExceptionInformation { get; set; }

        /// <summary>
        ///     Gets or sets the bonus mode
        /// </summary>
        public BonusMode Mode { get; set; }

        /// <summary>
        ///     Gets the ID for the message.  Empty if no message should be displayed
        /// </summary>
        public Guid DisplayMessageId { get; set; }

        /// <summary>
        ///     Gets or sets the message to be displayed
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Gets or sets the message duration
        /// </summary>
        public TimeSpan MessageDuration { get; set; }

        /// <summary>
        ///     Gets or sets the number of games that this mode can be applied to. Only used when Mode ==
        ///     BonusMode.MultipleJackpotTime
        /// </summary>
        public int MjtNumberOfGames { get; set; }

        /// <summary>
        ///     Gets or sets the Multiplier value to multiply wins by. Only used when Mode == BonusMode.MultipleJackpotTime
        /// </summary>
        public int MjtWinMultiplier { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the smallest win, inclusive, that is eligible to be multiplied. Only used when Mode ==
        ///     BonusMode.MultipleJackpotTime
        /// </summary>
        public long MjtMinimumWin { get; set; }

        /// <summary>
        ///     Gets or sets the maximum win, inclusive, that is eligible to be multiplied. Only used when Mode ==
        ///     BonusMode.MultipleJackpotTime
        /// </summary>
        public long MjtMaximumWin { get; set; }

        /// <summary>
        ///     Gets or sets how to limit the wagers for the games played. Only used when Mode == BonusMode.MultipleJackpotTime
        /// </summary>
        public WagerRestriction MjtWagerRestriction { get; set; }

        /// <summary>
        ///     Gets or sets the required wager amount for all Mjt game rounds
        /// </summary>
        public long MjtRequiredWager { get; set; }

        /// <summary>
        ///     Gets or sets the amount wagered during the bonus mode. Only used when Mode == BonusMode.MultipleJackpotTime
        /// </summary>
        public long MjtAmountWagered { get; set; }

        /// <summary>
        ///     Gets or sets the number of games played. Only used when Mode == BonusMode.MultipleJackpotTime
        /// </summary>
        public int MjtBonusGamesPlayed { get; set; }

        /// <summary>
        ///     Gets or sets the number of games that resulted in a multiplied win. Only used when Mode ==
        ///     BonusMode.MultipleJackpotTime
        /// </summary>
        public int MjtBonusGamesPaid { get; set; }

        /// <summary>
        ///     Gets or sets the underlying request
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// Gets or sets the source id
        /// </summary>
        public string SourceID { get; set; }

        /// <summary>
        /// Gets or sets the jackpot number
        /// </summary>
        public int JackpotNumber { get; set; }

        /// <summary>
        ///     Gets the total authorized amount for the bonus
        /// </summary>
        public long TotalAmount => Mode == BonusMode.Standard || Mode == BonusMode.NonDeductible ||
                                   Mode == BonusMode.WagerMatchAllAtOnce || Mode == BonusMode.GameWin
            ? CashableAmount + NonCashAmount + PromoAmount
            : 0;

        /// <summary>
        ///     Gets the wager match award amount
        /// </summary>
        public long WagerMatchAwardAmount => Mode == BonusMode.WagerMatch
            ? CashableAmount + NonCashAmount + PromoAmount
            : 0;

        /// <summary>
        ///    Get the last Authorized Cashable Amount for recovery
        ///    This is being used in WagerMatch bonus recovery
        /// </summary>
        public long LastAuthorizedCashableAmount { get; set; }

        /// <summary>
        ///    Get the last Authorized NonCash Amount for recovery
        ///    This is being used in WagerMatch bonus recovery
        /// </summary>
        public long LastAuthorizedNonCashAmount { get; set; }

        /// <summary>
        ///    Get the last Authorized Promo Amount for recovery
        ///    This is being used in WagerMatch bonus recovery
        /// </summary>
        public long LastAuthorizedPromoAmount { get; set; }

        /// <summary>
        ///     Gets and sets the related protocol
        /// </summary>
        public CommsProtocol Protocol { get; set; }

        /// <inheritdoc />
        public IEnumerable<long> AssociatedTransactions { get; set; }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="bonusTransaction1">The first transaction</param>
        /// <param name="bonusTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(BonusTransaction bonusTransaction1, BonusTransaction bonusTransaction2)
        {
            if (ReferenceEquals(bonusTransaction1, bonusTransaction2))
            {
                return true;
            }

            if (bonusTransaction1 is null || bonusTransaction2 is null)
            {
                return false;
            }

            return bonusTransaction1.Equals(bonusTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="bonusTransaction1">The first transaction</param>
        /// <param name="bonusTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(BonusTransaction bonusTransaction1, BonusTransaction bonusTransaction2)
        {
            return !(bonusTransaction1 == bonusTransaction2);
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return new
                BonusTransaction(
                    DeviceId,
                    TransactionDateTime,
                    BonusId,
                    CashableAmount,
                    NonCashAmount,
                    PromoAmount,
                    GameId,
                    Denom,
                    PayMethod)
            {
                TransactionId = TransactionId,
                LogSequence = LogSequence,
                State = State,
                IdRequired = IdRequired,
                IdNumber = IdNumber,
                PlayerId = PlayerId,
                PaidCashableAmount = PaidCashableAmount,
                PaidNonCashAmount = PaidNonCashAmount,
                PaidPromoAmount = PaidPromoAmount,
                PaidDateTime = PaidDateTime,
                LastUpdate = LastUpdate,
                Exception = Exception,
                ExceptionInformation = ExceptionInformation,
                Mode = Mode,
                DisplayMessageId = DisplayMessageId,
                Message = Message,
                MessageDuration = MessageDuration,
                MjtAmountWagered = MjtAmountWagered,
                MjtBonusGamesPaid = MjtBonusGamesPaid,
                MjtBonusGamesPlayed = MjtBonusGamesPlayed,
                MjtMaximumWin = MjtMaximumWin,
                MjtMinimumWin = MjtMinimumWin,
                MjtNumberOfGames = MjtNumberOfGames,
                MjtWagerRestriction = MjtWagerRestriction,
                MjtRequiredWager = MjtRequiredWager,
                MjtWinMultiplier = MjtWinMultiplier,
                Request = Request,
                SourceID = SourceID,
                JackpotNumber = JackpotNumber,
                AssociatedTransactions = AssociatedTransactions.ToList(),
                Protocol = Protocol,
                LastAuthorizedNonCashAmount = LastAuthorizedNonCashAmount,
                LastAuthorizedCashableAmount = LastAuthorizedCashableAmount,
                LastAuthorizedPromoAmount = LastAuthorizedPromoAmount
            };
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            BonusId = (string)values["BonusId"];
            State = (BonusState)values["State"];
            CashableAmount = (long)values["CashableAmount"];
            NonCashAmount = (long)values["NonCashAmount"];
            PromoAmount = (long)values["PromoAmount"];
            GameId = (int)values["GameId"];
            Denom = (long)values["Denom"];
            PayMethod = (PayMethod)values["PayMethod"];
            IdRequired = (bool)values["IdRequired"];
            IdNumber = (string)values["IdNumber"];
            PlayerId = (string)values["PlayerId"];
            PaidCashableAmount = (long)values["PaidCashableAmount"];
            PaidNonCashAmount = (long)values["PaidNonCashAmount"];
            PaidPromoAmount = (long)values["PaidPromoAmount"];
            PaidDateTime = (DateTime)values["PaidDateTime"];
            LastUpdate = (DateTime)values["LastUpdate"];
            Exception = (int)values["Exception"];
            ExceptionInformation = (int)values["ExceptionInformation"];
            Mode = (BonusMode)values["Mode"];
            Message = (string)values["Message"];
            MjtAmountWagered = (long)values["MjtAmountWagered"];
            MjtBonusGamesPaid = (int)values["MjtBonusGamesPaid"];
            MjtBonusGamesPlayed = (int)values["MjtBonusGamesPlayed"];
            MjtMaximumWin = (long)values["MjtMaximumWin"];
            MjtMinimumWin = (long)values["MjtMinimumWin"];
            MjtNumberOfGames = (int)values["MjtNumberOfGames"];
            MjtWagerRestriction = (WagerRestriction)values["MjtWagerRestriction"];
            MjtRequiredWager = (long)values["MjtRequiredWager"];
            MjtWinMultiplier = (int)values["MjtWinMultiplier"];
            Request = (string)values["Request"];
            MessageDuration = TimeSpan.FromTicks((long)values["MessageDuration"]);
            JackpotNumber = (int)values[nameof(JackpotNumber)];
            SourceID = (string)values[nameof(SourceID)];
            LastAuthorizedCashableAmount= (long)values["LastAuthorizedCashableAmount"];
            LastAuthorizedNonCashAmount = (long)values["LastAuthorizedNonCashAmount"];
            LastAuthorizedPromoAmount = (long)values["LastAuthorizedPromoAmount"];

            var displayMessageId = values["DisplayMessageId"];
            if (displayMessageId != null)
            {
                DisplayMessageId = (Guid)displayMessageId;
            }

            var associated = (string)values["AssociatedTransactions"];
            AssociatedTransactions = !string.IsNullOrEmpty(associated)
                ? JsonConvert.DeserializeObject<List<long>>(associated)
                : Enumerable.Empty<long>();

            Protocol = values.TryGetValue(nameof(Protocol), out var protocol) ? (CommsProtocol)protocol : CommsProtocol.None;
            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);

            using (var transaction = block.StartTransaction())
            {
                transaction[element, "BonusId"] = BonusId;
                transaction[element, "State"] = (int)State;
                transaction[element, "CashableAmount"] = CashableAmount;
                transaction[element, "NonCashAmount"] = NonCashAmount;
                transaction[element, "PromoAmount"] = PromoAmount;
                transaction[element, "GameId"] = GameId;
                transaction[element, "Denom"] = Denom;
                transaction[element, "PayMethod"] = (int)PayMethod;
                transaction[element, "IdRequired"] = IdRequired;
                transaction[element, "IdNumber"] = IdNumber;
                transaction[element, "PlayerId"] = PlayerId;
                transaction[element, "PaidCashableAmount"] = PaidCashableAmount;
                transaction[element, "PaidNonCashAmount"] = PaidNonCashAmount;
                transaction[element, "PaidPromoAmount"] = PaidPromoAmount;
                transaction[element, "LastUpdate"] = LastUpdate;
                transaction[element, "PaidDateTime"] = PaidDateTime;
                transaction[element, "Exception"] = Exception;
                transaction[element, "ExceptionInformation"] = ExceptionInformation;
                transaction[element, "Mode"] = (int)Mode;
                transaction[element, "DisplayMessageId"] = DisplayMessageId;
                transaction[element, "Message"] = Message;
                transaction[element, "MjtAmountWagered"] = MjtAmountWagered;
                transaction[element, "MjtBonusGamesPaid"] = MjtBonusGamesPaid;
                transaction[element, "MjtBonusGamesPlayed"] = MjtBonusGamesPlayed;
                transaction[element, "MjtMaximumWin"] = MjtMaximumWin;
                transaction[element, "MjtMinimumWin"] = MjtMinimumWin;
                transaction[element, "MjtNumberOfGames"] = MjtNumberOfGames;
                transaction[element, "MjtWagerRestriction"] = MjtWagerRestriction;
                transaction[element, "MjtRequiredWager"] = MjtRequiredWager;
                transaction[element, "MjtWinMultiplier"] = MjtWinMultiplier;
                transaction[element, "Request"] = Request;
                transaction[element, "AssociatedTransactions"] =
                    JsonConvert.SerializeObject(AssociatedTransactions, Formatting.None);
                transaction[element, "MessageDuration"] = MessageDuration.Ticks;
                transaction[element, nameof(JackpotNumber)] = JackpotNumber;
                transaction[element, nameof(SourceID)] = SourceID;
                transaction[element, nameof(Protocol)] = (int)Protocol;
                transaction[element, "LastAuthorizedCashableAmount"] = LastAuthorizedCashableAmount;
                transaction[element, "LastAuthorizedNonCashAmount"] = LastAuthorizedNonCashAmount;
                transaction[element, "LastAuthorizedPromoAmount"] = LastAuthorizedPromoAmount;
                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{GetType()} [DeviceId={DeviceId}, " +
                $"LogSequence={LogSequence}, " +
                $"DateTime={TransactionDateTime}, " +
                $"TransactionId={TransactionId}, " +
                $"BonusId={BonusId}, " +
                $"Mode={Mode}, " +
                $"PayMethod={PayMethod}, " +
                $"State={State}, " +
                $"CashableAmount={CashableAmount}, " +
                $"NonCashAmount={NonCashAmount}, " +
                $"PromoAmount={PromoAmount}, " +
                $"PayMethod={PayMethod}, " +
                $"PaidAmount={PaidAmount}, " +
                $"PaidDateTime={PaidDateTime}, " +
                $"DisplayMessageId={DisplayMessageId}, " +
                $"SourceID={SourceID}, " +
                $"JackpotNumber={JackpotNumber}, " +
                $"Protocol={Protocol}, " +
                $"LastAuthorizedCashableAmount={LastAuthorizedCashableAmount}, " +
                $"LastAuthorizedNonCashAmount={LastAuthorizedNonCashAmount}, " +
                $"LastAuthorizedPromoAmount={LastAuthorizedPromoAmount}]";
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as BonusTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Tuple.Create(TransactionId, TransactionDateTime).GetHashCode();
        }

        /// <summary>
        ///     Checks that two BonusTransactions are the same by value.
        /// </summary>
        /// <param name="bonusTransaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(BonusTransaction bonusTransaction)
        {
            return base.Equals(bonusTransaction);
        }
    }
}