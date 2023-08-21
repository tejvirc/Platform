namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using Hardware.Contracts.Persistence;
    using log4net;

    /// <summary>
    ///     BaseTransaction encapsulates the properties common to all transactions and handles their persistence.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class provides a base implementation of the ITransaction interface for cases when transactions are to be
    ///         retained in persistent
    ///         storage. In addition to properties common to all transactions, it has virtual methods that set and retrieve
    ///         properties from a block
    ///         of persistent storage data. It also has base implementations of equality operators and a ToString method. The
    ///         base implementations of
    ///         these methods deal with the common properties. Classes that inherit from BaseTransaction should deal with their
    ///         additional properties
    ///         in the method overrides and call up to the base class methods for handling of the common properties.
    ///     </para>
    ///     <para>
    ///         In order for the TransactionHistory component to make a TransactionHistoryProvider object from a class derived
    ///         from BaseTransaction,
    ///         an addin file needs to be created so that mono-addins can find specifications regarding class and other
    ///         information. There are three
    ///         essential ingredients to the addin file for any new transaction type.  The first is the import statement of the
    ///         dll containing the
    ///         definition of the transaction object. The second is a dependency statement to the TransactionHistory addin,
    ///         since this addin defines the
    ///         extension point (/Accounting/TransactionHistories) that the new transaction type will need to define its
    ///         storage characteristics. The third
    ///         is the extension point itself.
    ///     </para>
    ///     <para>
    ///         Components that use classes derived from BaseTransaction should be able to obtain a Persistent Storage Accessor
    ///         from the Persistent Storage
    ///         Manager. This accessor is passed into SetPersistence and ReceivePersistence to set or receive values of
    ///         properties in persistent storage.
    ///         To do so, each derived class must have an associated XML file defining the layout of items in persistent
    ///         storage. By convention, the
    ///         name of the XML file is the full name of the class followed by the file type; i.e. ".xml". An example can be
    ///         found in the
    ///         TransactionHistoryInterfacesUnitTest project.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     An example of a class derived from BaseTransaction is shown below. It adds Barcode to the common properties.
    ///     <code>
    ///  public class HandpayTransaction : BaseTransaction
    ///  {
    ///     public HandpayTransaction()
    ///     {
    ///     }
    ///     public HandpayTransaction(
    ///         int deviceId,
    ///         DateTime transactionDateTime,
    ///         long amount,
    ///         AccountType accountType,
    ///         string barcode)
    ///         : base(deviceId, transactionDateTime, amount, accountType)
    ///     {
    ///         if (string.IsNullOrEmpty(barcode) || barcode.Equals("0"))
    ///         {
    ///             Barcode = null;
    ///         }
    ///         else
    ///         {
    ///             Barcode = barcode;
    ///         }
    ///     }
    ///     public override string Name
    ///     {
    ///         get { return Localization.Resources.TransactionName; }
    ///     }
    ///     public string Barcode
    ///     {
    ///         get;
    ///         private set;
    ///     }
    ///     #region Public Static Methods
    ///     public static bool operator ==(HandpayTransaction transaction1, HandpayTransaction transaction2)
    ///     {
    ///         if (object.ReferenceEquals(transaction1, transaction2))
    ///         {
    ///             return true;
    ///         }
    ///         if ((object)transaction1 == null || (object)transaction2 == null)
    ///         {
    ///             return false;
    ///         }
    ///         return transaction1.Equals(transaction2);
    ///     }
    ///     public static bool operator !=(HandpayTransaction transaction1, HandpayTransaction transaction2)
    ///     {
    ///         return !(transaction1 == transaction2);
    ///     }
    ///     #endregion
    ///     #region BaseTransaction Members
    ///     public override bool ReceivePersistence(IPersistentStorageAccessor block, int element)
    ///     {
    ///         bool success = base.ReceivePersistence(block, element);
    ///         // Retrieve data specific to this transaction type
    ///         int originalBarcodeLength = (int)block[element, "BarcodeLength"];
    ///         long numericBarcode = (long)block[element, "Barcode"];
    ///         Barcode = numericBarcode.ToString(CultureInfo.InvariantCulture);
    ///         if (Barcode.Length &lt; originalBarcodeLength)
    ///         {
    ///             Barcode = Barcode.Insert(0, new string('0', originalBarcodeLength - Barcode.Length));
    ///         }
    ///         else if (originalBarcodeLength == 0)
    ///         {
    ///             Barcode = null;
    ///         }
    ///         return success;
    ///     }
    ///     public override void SetPersistence(IPersistentStorageAccessor block, int element)
    ///     {
    ///         base.SetPersistence(block, element);
    ///         // Add data specific to this transaction type
    ///         block.StartUpdate(true);
    ///         block[element, "BarcodeLength"] = Barcode == null ? 0 : Barcode.Length;
    ///         block[element, "Barcode"] = Convert.ToInt64(Barcode, CultureInfo.InvariantCulture);
    ///         block.Commit();
    ///     }
    ///     #endregion
    ///     #region Public Overrides
    ///     public override string ToString()
    ///     {
    ///         return string.Format(
    ///             CultureInfo.InvariantCulture,
    ///             "{0} [DeviceId={1}, LogSequence={2}, DateTime={3}, TransactionId={4}, Amount={5}, TypeOfAccount={6}, Barcode={7}]",
    ///             GetType(),
    ///             DeviceId,
    ///             LogSequence,
    ///             TransactionDateTime.ToString(CultureInfo.InvariantCulture),
    ///             TransactionId,
    ///             Amount,
    ///             TypeOfAccount,
    ///             Barcode == null ? string.Empty : Barcode);
    ///     }
    ///     public override bool Equals(object obj)
    ///     {
    ///         return Equals(obj as HandpayTransaction);
    ///     }
    ///     public override int GetHashCode()
    ///     {
    ///         return Barcode == null ? base.GetHashCode() : base.GetHashCode() ^ Barcode.GetHashCode();
    ///     }
    ///     public override object Clone()
    ///     {
    ///         HandpayTransaction copy = new HandpayTransaction(DeviceId, TransactionDateTime, Amount, TypeOfAccount, Barcode);
    ///         copy.LogSequence = LogSequence;
    ///         copy.TransactionId = TransactionId;
    ///         return copy;
    ///     }
    ///     #endregion
    ///     #region Public Methods
    ///     public bool Equals(HandpayTransaction transaction)
    ///     {
    ///         return base.Equals(transaction) &amp;&amp;
    ///             Barcode == transaction.Barcode;
    ///     }
    ///     #endregion
    ///  }
    ///  </code>
    /// </example>
    [Serializable]
    public abstract class BaseTransaction : ITransaction
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseTransaction" /> class.
        /// </summary>
        protected BaseTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">Value to set as DeviceId.</param>
        /// <param name="transactionDateTime">Value to set as TransactionDateTime.</param>
        protected BaseTransaction(int deviceId, DateTime transactionDateTime)
        {
            DeviceId = deviceId;
            TransactionDateTime = transactionDateTime;
        }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public int DeviceId { get; private set; }

        /// <inheritdoc />
        public long LogSequence { get; set; }

        /// <inheritdoc />
        public DateTime TransactionDateTime { get; private set; }

        /// <inheritdoc />
        public long TransactionId { get; set; }

        /// <inheritdoc />
        public abstract object Clone();

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="baseTransaction1">The first transaction</param>
        /// <param name="baseTransaction2">The second transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public static bool operator ==(BaseTransaction baseTransaction1, BaseTransaction baseTransaction2)
        {
            if (ReferenceEquals(baseTransaction1, baseTransaction2))
            {
                return true;
            }

            if (baseTransaction1 is null || baseTransaction2 is null)
            {
                return false;
            }

            return baseTransaction1.Equals(baseTransaction2);
        }

        /// <summary>
        ///     Checks two transactions to see if they are different.
        /// </summary>
        /// <param name="baseTransaction1">The first transaction</param>
        /// <param name="baseTransaction2">The second transaction</param>
        /// <returns>False if the object are equivalent, true otherwise.</returns>
        public static bool operator !=(BaseTransaction baseTransaction1, BaseTransaction baseTransaction2)
        {
            return !(baseTransaction1 == baseTransaction2);
        }

        /// <summary>
        ///     Returns a human-readable representation of the BaseTransaction class.
        /// </summary>
        /// <returns>A human-readable string.</returns>
        public override string ToString()
        {
            var message = new StringBuilder();

            message.Append("This transaction has the following values:\n");
            message.AppendFormat(CultureInfo.CurrentCulture, "DeviceId: {0}\n", DeviceId);
            message.AppendFormat(CultureInfo.CurrentCulture, "LogSequence: {0}\n", LogSequence);
            message.AppendFormat(CultureInfo.CurrentCulture, "TransactionDateTime: {0}\n", TransactionDateTime.ToLocalTime().ToLongDateString());
            message.AppendFormat(CultureInfo.CurrentCulture, "Detailed DateTime: {0:s}{0:.fffzzz}\n", TransactionDateTime.ToLocalTime());
            message.AppendFormat(CultureInfo.CurrentCulture, "TransactionId: {0}\n", TransactionId);

            return message.ToString();
        }

        /// <summary>
        ///     Checks for equivalency between two transactions.
        /// </summary>
        /// <param name="obj">The object to be checked.</param>
        /// <returns>True if the objects are equivalent, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BaseTransaction);
        }

        /// <summary>
        ///     Overrides the hashcode.
        /// </summary>
        /// <returns>
        ///     Returns the hashcode based on the transaction id,
        ///     the amount, the type of account, the device id, and the ticks
        ///     in the date time all xor'd together.
        /// </returns>
        public override int GetHashCode()
        {
            return TransactionId.GetHashCode() ^ DeviceId.GetHashCode() ^ TransactionDateTime.Ticks.GetHashCode();
        }

        /// <summary>
        ///     Checks two transactions to see if they are the same.
        /// </summary>
        /// <param name="baseTransaction">The first transaction</param>
        /// <returns>True if the object are equivalent, false otherwise.</returns>
        public bool Equals(BaseTransaction baseTransaction)
        {
            return baseTransaction != null &&
                   DeviceId == baseTransaction.DeviceId &&
                   LogSequence == baseTransaction.LogSequence &&
                   Name == baseTransaction.Name &&
                   TransactionDateTime == baseTransaction.TransactionDateTime &&
                   TransactionId == baseTransaction.TransactionId;
        }

        /// <summary>
        ///     Provides the object with a collection containing its persisted data.
        /// </summary>
        /// <returns>True if block/element has valid data.</returns>
        public virtual bool SetData(IDictionary<string, object> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(values));
            }

            DeviceId = (int)values["DeviceId"];
            LogSequence = (long)values["LogSequence"];
            TransactionDateTime = (DateTime)values["TransactionDateTime"];
            TransactionId = (long)values["TransactionId"];

            return LogSequence > 0;
        }

        /// <summary>
        ///     Puts the object's data into the associated block of persistent storage.
        /// </summary>
        /// <param name="block">An accessor for the block.</param>
        /// <param name="element">The index of the element to use.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the element index is out of range of the given block.</exception>
        /// <exception cref="KeyNotFoundException">
        ///     Thrown when the key given to get a value within one of the block's elements does
        ///     not exist.
        /// </exception>
        public virtual void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            if (element >= block.Count || element < 0)
            {
                Logger.Fatal($"Element index, {element}, is outside of the block range, {block.Count}.");
                throw new ArgumentOutOfRangeException(nameof(element), @"Element index out of range of storage block.");
            }

            using (var transaction = block.StartTransaction())
            {
                transaction[element, "DeviceId"] = DeviceId;
                transaction[element, "LogSequence"] = LogSequence;
                transaction[element, "TransactionDateTime"] = TransactionDateTime;
                transaction[element, "TransactionId"] = TransactionId;
                transaction.Commit();
            }
        }
    }
}
