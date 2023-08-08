namespace Aristocrat.Monaco.Accounting.Contracts.TransferOut
{
    /// <summary>
    ///     Defines the result for a call to <see cref="ITransferOutProvider.Transfer" />
    /// </summary>
    public class TransferResult
    {
        /// <summary>
        ///     Defines a failed transfer
        /// </summary>
        public static readonly TransferResult Failed = new TransferResult(0, 0, 0);

        private readonly bool _zeroTotalOk;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferResult" /> class.
        /// </summary>
        /// <param name="transferredCashable">The amount transferred out in millicents of cashable credits.</param>
        /// <param name="transferredPromo">The amount transferred out in millicents of promotional credits.</param>
        /// <param name="transferredNonCash">The amount transferred out in millicents of non-cashable credits.</param>
        public TransferResult(long transferredCashable, long transferredPromo, long transferredNonCash)
            : this(transferredCashable, transferredPromo, transferredNonCash, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferResult" /> class.
        /// </summary>
        /// <param name="transferredCashable">The amount transferred out in millicents of cashable credits.</param>
        /// <param name="transferredPromo">The amount transferred out in millicents of promotional credits.</param>
        /// <param name="transferredNonCash">The amount transferred out in millicents of non-cashable credits.</param>
        /// <param name="zeroTotalOk">Used to indicate that a <see cref="Total" /> of zero can be a successful transfer.</param>
        public TransferResult(long transferredCashable, long transferredPromo, long transferredNonCash, bool zeroTotalOk)
            : this(transferredCashable, transferredPromo, transferredNonCash, zeroTotalOk, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferResult" /> class.
        /// </summary>
        /// <param name="transferredCashable">The amount transferred out in millicents of cashable credits.</param>
        /// <param name="transferredPromo">The amount transferred out in millicents of promotional credits.</param>
        /// <param name="transferredNonCash">The amount transferred out in millicents of non-cashable credits.</param>
        /// <param name="zeroTotalOk">Used to indicate that a <see cref="Total" /> of zero can be a successful transfer.</param>
        /// <param name="isPartialTransferOut"> Used to check weather the transfer is partial.</param>
        public TransferResult(long transferredCashable, long transferredPromo, long transferredNonCash, bool zeroTotalOk, bool isPartialTransferOut)
        {
            TransferredCashable = transferredCashable;
            TransferredPromo = transferredPromo;
            TransferredNonCash = transferredNonCash;

            _zeroTotalOk = zeroTotalOk;
            IsPartialTransferOut = isPartialTransferOut;
        }
        /// <summary>
        ///     Gets a value indicating whether or not the transfer was successful
        /// </summary>
        public bool Success => Total > 0 || _zeroTotalOk;

        /// <summary>
        ///     Gets the amount of cashable credits transferred out in millicents
        /// </summary>
        public long TransferredCashable { get; }

        /// <summary>
        ///     Gets the amount of promotional credits transferred out in millicents
        /// </summary>
        public long TransferredPromo { get; }

        /// <summary>
        ///     Gets the amount of non-cashable credits transferred out in millicents
        /// </summary>
        public long TransferredNonCash { get; }

        /// <summary>
        ///     Gets the total amount transferred out
        /// </summary>
        public long Total => TransferredCashable + TransferredPromo + TransferredNonCash;
        /// <summary>
        ///     check weather the transfer is partial.
        /// </summary>
        public bool IsPartialTransferOut { get; }
    }
}