namespace Aristocrat.Monaco.Accounting.Contracts.Hopper
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Localization.Properties;

    /// <summary>
    ///     HopperRefillTransaction defines the data necessary to store a Hopper Refill.
    /// </summary>
    [Serializable]
    public class HopperRefillTransaction : BaseTransaction
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HopperRefillTransaction" /> class.
        /// </summary>
        public HopperRefillTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HopperRefillTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">Value to set as DeviceId.</param>
        /// <param name="transactionDateTime">Value to set as TransactionDateTime.</param>
        /// <param name="lastRefillValue">Value to set as hopper last refill value.</param>
        public HopperRefillTransaction(
            int deviceId,
            DateTime transactionDateTime,
            long lastRefillValue)
            : base(deviceId, transactionDateTime)
        {
            LastRefillValue = lastRefillValue;
        }

        /// <inheritdoc />
        public override string Name => "Hopper Refill Transaction";
            //TBC Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HopperRefillTransactionName);

        /// <summary>
        ///     Gets the type of account.
        /// </summary>
        public AccountType TypeOfAccount => AccountType.Cashable;
        
        /// <summary>
        ///     Gets or sets Last Refill Value
        /// </summary>
        public long LastRefillValue { get; set; }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="hopperRefillTransaction1">The first transaction</param>
        /// <param name="hopperRefillTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(HopperRefillTransaction hopperRefillTransaction1, HopperRefillTransaction hopperRefillTransaction2)
        {
            if (ReferenceEquals(hopperRefillTransaction1, hopperRefillTransaction2))
            {
                return true;
            }

            if (hopperRefillTransaction1 is null || hopperRefillTransaction2 is null)
            {
                return false;
            }

            return hopperRefillTransaction1.Equals(hopperRefillTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="hopperRefillTransaction1">The first transaction</param>
        /// <param name="hopperRefillTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(HopperRefillTransaction hopperRefillTransaction1, HopperRefillTransaction hopperRefillTransaction2)
        {
            return !(hopperRefillTransaction1 == hopperRefillTransaction2);
        }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }
            LastRefillValue = (long)values["LastRefillValue"];

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);
            using (var transaction = block.StartTransaction())
            {
                transaction[element, "LastRefillValue"] = LastRefillValue;
                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} [DeviceId={1}, LogSequence={2}, DateTime={3}, TransactionId={4}, TypeOfAccount={5}, LastRefillValue={6}]",
                GetType(),
                DeviceId,
                LogSequence,
                TransactionDateTime.ToString(CultureInfo.InvariantCulture),
                TransactionId,
                TypeOfAccount,
                LastRefillValue);

            return builder.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as HopperRefillTransaction);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        public override object Clone()
        {
            return new HopperRefillTransaction(DeviceId, TransactionDateTime, LastRefillValue)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                LastRefillValue = LastRefillValue
            };
        }

        /// <summary>
        ///     Checks that two HopperRefillTransaction are the same by value.
        /// </summary>
        /// <param name="hopperRefillTransaction">The transaction to check against.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals(HopperRefillTransaction hopperRefillTransaction)
        {
            return base.Equals(hopperRefillTransaction);
        }
    }
}
