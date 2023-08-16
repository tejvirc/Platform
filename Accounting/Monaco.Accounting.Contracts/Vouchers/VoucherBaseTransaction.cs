namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Hardware.Contracts.Persistence;
    using Newtonsoft.Json;
    using Transactions;

    /// <summary>
    ///     VoucherBaseTransaction encapsulates and persists the data for a single
    ///     voucher transaction.
    /// </summary>
    [Serializable]
    public abstract class VoucherBaseTransaction : BaseTransaction, ITransactionConnector
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherBaseTransaction" /> class.
        ///     This constructor is only used by the transaction framework.
        /// </summary>
        protected VoucherBaseTransaction()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherBaseTransaction" /> class.
        /// </summary>
        /// <param name="deviceId">The transaction device identifier</param>
        /// <param name="transactionDateTime">The date and time of the transaction</param>
        /// <param name="amount">The currency amount of the voucher</param>
        /// <param name="accountType">The type of credits on the voucher</param>
        /// <param name="barcode">The barcode of the issued voucher</param>
        protected VoucherBaseTransaction(
            int deviceId,
            DateTime transactionDateTime,
            long amount,
            AccountType accountType,
            string barcode)
            : base(deviceId, transactionDateTime)
        {
            Amount = amount;
            TypeOfAccount = accountType;
            Barcode = barcode;
            AssociatedTransactions = Enumerable.Empty<long>().ToList();
            LogDisplayType = string.Empty;
        }

        /// <summary>
        ///     Gets the amount of money involved in the transaction.
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Gets the type of account.
        /// </summary>
        public AccountType TypeOfAccount { get; set; }

        /// <summary>
        ///     Gets the barcode for the voucher.
        /// </summary>
        public string Barcode { get; private set; }

        /// <summary>
        ///     Gets or sets the associated transaction.  This includes items such as game play, vouchers, etc.
        ///     It is possible that the associated transactions are not available since the associated log may have rolled over
        /// </summary>
        public IEnumerable<long> AssociatedTransactions { get; set; }

        /// <summary>
        ///     Gets or sets the Log Display Type.  This allows for overriding the log voucher type value, used for mixed credit types.
        /// </summary>
        public string LogDisplayType { get; set; }

        /// <inheritdoc />
        public override bool SetData(IDictionary<string, object> values)
        {
            if (!base.SetData(values))
            {
                return false;
            }

            // Retrieve data specific to this transaction type
            var originalBarcodeLength = (int)values["BarcodeLength"];

            Amount = (long)values["Amount"];
            TypeOfAccount = (AccountType)(byte)values["TypeOfAccount"];

            var associated = (string)values["AssociatedTransactions"];
            AssociatedTransactions = !string.IsNullOrEmpty(associated)
                ? JsonConvert.DeserializeObject<List<long>>(associated)
                : Enumerable.Empty<long>();

            LogDisplayType = (string)values["LogDisplayType"];

            if (originalBarcodeLength == 0)
            {
                Barcode = null;
            }
            else
            {
                if (values.TryGetValue("Barcode", out var rawBarcode))
                {
                    // convert the barcode from a number back to a string with the appropriate number of leading zeros
                    Barcode = ((long)rawBarcode).ToString(CultureInfo.InvariantCulture);
                    if (Barcode.Length < originalBarcodeLength)
                    {
                        Barcode = Barcode.Insert(0, new string('0', originalBarcodeLength - Barcode.Length));
                    }
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override void SetPersistence(IPersistentStorageAccessor block, int element)
        {
            base.SetPersistence(block, element);

            using (var transaction = block.StartTransaction())
            {
                transaction[element, "BarcodeLength"] = Barcode.Length;
                transaction[element, "Barcode"] = Convert.ToInt64(Barcode, CultureInfo.InvariantCulture);
                transaction[element, "Amount"] = Amount;
                transaction[element, "TypeOfAccount"] = (byte)TypeOfAccount;
                transaction[element, "AssociatedTransactions"] =
                    JsonConvert.SerializeObject(AssociatedTransactions, Formatting.None);
                transaction[element, "LogDisplayType"] = LogDisplayType;

                transaction.Commit();
            }
        }
    }
}