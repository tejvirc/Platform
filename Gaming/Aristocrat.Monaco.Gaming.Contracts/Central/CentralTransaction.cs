namespace Aristocrat.Monaco.Gaming.Contracts.Central
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
    ///     The <see cref="CentralTransaction" /> contains all of the data for a games central outcome
    /// </summary>
    public class CentralTransaction : BaseTransaction, ITransactionConnector
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CentralTransaction" /> class.
        /// </summary>
        public CentralTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CentralTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="gameId">The originating game play Id</param>
        /// <param name="denomination">The originating denomination</param>
        /// <param name="wagerCategory">The originating wager category</param>
        /// <param name="wagerAmount">The initial wager amount</param>
        /// <param name="outcomesRequested">The number of requested outcomes</param>
        public CentralTransaction(
            int deviceId,
            DateTime transactionDateTime,
            int gameId,
            long denomination,
            string wagerCategory,
            long wagerAmount,
            int outcomesRequested)
            : base(deviceId, transactionDateTime)
        {
            GameId = gameId;
            Denomination = denomination;
            WagerCategory = wagerCategory;
            WagerAmount = wagerAmount;
            OutcomesRequested = outcomesRequested;

            OutcomeState = OutcomeState.Requested;
            Outcomes = Enumerable.Empty<Outcome>();
            AssociatedTransactions = Enumerable.Empty<long>();
            Descriptions = Enumerable.Empty<IOutcomeDescription>();
        }

        /// <summary>
        ///     Gets or sets the current outcome state
        /// </summary>
        public OutcomeState OutcomeState { get; set; }

        /// <summary>
        ///     Gets the game Id associated with the outcome
        /// </summary>
        public int GameId { get; private set; }

        /// <summary>
        ///     Gets the denomination associated with the outcome
        /// </summary>
        public long Denomination { get; private set; }

        /// <summary>
        ///     Gets the wager category associated with the outcome
        /// </summary>
        public string WagerCategory { get; private set; }

        /// <summary>
        ///     Gets the wager amount
        /// </summary>
        public long WagerAmount { get; private set; }

        /// <summary>
        ///     Gets the number of requested outcomes
        /// </summary>
        public int OutcomesRequested { get; private set; }

        /// <summary>
        ///     Gets or sets the associated outcomes
        /// </summary>
        public IEnumerable<Outcome> Outcomes { get; set; }

        /// <summary>
        ///     Gets or sets the outcome exception
        /// </summary>
        public OutcomeException Exception { get; set; }

        /// <inheritdoc />
        public override string Name =>
            Localizer.For(CultureFor.Player).GetString(ResourceKeys.CentralTransactionName);

        /// <inheritdoc />
        public IEnumerable<long> AssociatedTransactions { get; set; }

        /// <summary>
        /// </summary>
        public IEnumerable<IOutcomeDescription> Descriptions { get; set; }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="transaction1">The first transaction</param>
        /// <param name="transaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(
            CentralTransaction transaction1,
            CentralTransaction transaction2)
        {
            if (ReferenceEquals(transaction1, transaction2))
            {
                return true;
            }

            if (transaction1 is null || transaction2 is null)
            {
                return false;
            }

            return transaction1.Equals(transaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="transaction1">The first transaction</param>
        /// <param name="transaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(
            CentralTransaction transaction1,
            CentralTransaction transaction2)
        {
            return !(transaction1 == transaction2);
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return new CentralTransaction(
                DeviceId,
                TransactionDateTime,
                GameId,
                Denomination,
                WagerCategory,
                WagerAmount,
                OutcomesRequested)
            {
                TransactionId = TransactionId,
                LogSequence = LogSequence,
                OutcomeState = OutcomeState,
                Outcomes = Outcomes.ToList(),
                Exception = Exception,
                AssociatedTransactions = AssociatedTransactions.ToList(),
                Descriptions = Descriptions.ToList()
            };
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            OutcomeState = (OutcomeState)values["OutcomeState"];
            GameId = (int)values["GameId"];
            Denomination = (long)values["Denomination"];
            WagerCategory = (string)values["WagerCategory"];
            WagerAmount = (long)values["WagerAmount"];
            OutcomesRequested = (int)values["OutcomesRequested"];

            var outcomes = (string)values["Outcomes"];
            Outcomes = !string.IsNullOrEmpty(outcomes)
                ? JsonConvert.DeserializeObject<List<Outcome>>(outcomes)
                : Enumerable.Empty<Outcome>();

            Exception = (OutcomeException)values["Exception"];

            var descriptions = (string)values["Descriptions"];
            Descriptions =  !string.IsNullOrEmpty(outcomes)
                ? JsonConvert.DeserializeObject<List<IOutcomeDescription>>(
                    descriptions,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })
                : Enumerable.Empty<IOutcomeDescription>();

            var associated = (string)values["AssociatedTransactions"];
            AssociatedTransactions = !string.IsNullOrEmpty(associated)
                ? JsonConvert.DeserializeObject<List<long>>(associated)
                : Enumerable.Empty<long>();

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);

            using (var transaction = block.StartTransaction())
            {
                transaction[element, "OutcomeState"] = OutcomeState;
                transaction[element, "GameId"] = GameId;
                transaction[element, "Denomination"] = Denomination;
                transaction[element, "WagerCategory"] = WagerCategory;
                transaction[element, "WagerAmount"] = WagerAmount;
                transaction[element, "OutcomesRequested"] = OutcomesRequested;
                transaction[element, "Outcomes"] = JsonConvert.SerializeObject(Outcomes, Formatting.None);
                transaction[element, "Descriptions"] = JsonConvert.SerializeObject(
                    Descriptions,
                    Formatting.None,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                transaction[element, "Exception"] = Exception;
                transaction[element, "AssociatedTransactions"] =
                    JsonConvert.SerializeObject(AssociatedTransactions, Formatting.None);

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{typeof(CentralTransaction)} [DeviceId={DeviceId}, LogSequence={LogSequence}, DateTime={TransactionDateTime}, TransactionId={TransactionId}, LogSequence={LogSequence}, GameId={GameId}, Denom={Denomination}, Wager={WagerAmount}, State={OutcomeState}, Exception={Exception}";
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as CentralTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (TransactionId, TransactionDateTime).GetHashCode();
        }

        /// <summary>
        ///     Checks that two CentralTransaction are the same by value.
        /// </summary>
        /// <param name="transaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(CentralTransaction transaction)
        {
            return base.Equals(transaction) &&
                   TransactionId == transaction.TransactionId;
        }
    }
}