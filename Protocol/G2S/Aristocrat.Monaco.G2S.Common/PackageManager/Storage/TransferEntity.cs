namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System;
    using System.Globalization;
    using System.Text;
    using Data.Model;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Transfer entity implementation.
    /// </summary>
    public class TransferEntity : BaseEntity
    {
        /// <summary>
        ///     Gets or sets transfer unique identifier.
        /// </summary>
        public int TransferId { get; set; }

        /// <summary>
        ///     Gets or sets package Id.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        ///     Gets or sets transfer location that is an IP address:port/path/filename or valid network URI address.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        ///     Gets or sets optional parameters required for the transfer of the package. Could be empty.
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        ///     Gets or sets reason code.
        /// </summary>
        public string ReasonCode { get; set; }

        /// <summary>
        ///     Gets or sets transfer type.
        /// </summary>
        public TransferType TransferType { get; set; }

        /// <summary>
        ///     Gets or sets transfer state.
        /// </summary>
        public TransferState State { get; set; }

        /// <summary>
        ///     Gets or sets Exception
        /// </summary>
        public int Exception { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to delete the package after transfer completes.
        /// </summary>
        public bool DeleteAfter { get; set; }

        /// <summary>
        ///     Gets or sets the package size.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the transfer has paused.
        /// </summary>
        public bool TransferPaused { get; set; }

        /// <summary>
        ///     Gets or sets the package transfer size.
        /// </summary>
        public long TransferSize { get; set; }

        /// <summary>
        ///     Gets or sets the transfer completed datetime.
        /// </summary>
        public DateTime? TransferCompletedDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the package validate datetime.
        /// </summary>
        public DateTime? PackageValidateDateTime { get; set; }

        /// <summary>
        ///     Returns a human-readable representation of a Transfer Entity.
        /// </summary>
        /// <returns>A human-readable string.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} [TransferId={1}, PackageId={2}, Location={3}, Parameters={4}, ReasonCode={5}, TransferType={6}, State={7}, Exception={8}, DeleteAfter={9}, Size={10}, TransferPaused={11}, TransferSize={12}, TransferCompletedDateTime={13}, PackageValidateDateTime={14}",
                GetType(),
                TransferId,
                PackageId,
                Location,
                Parameters,
                ReasonCode,
                TransferType,
                State,
                Exception,
                DeleteAfter,
                Size,
                TransferPaused,
                TransferSize,
                TransferCompletedDateTime?.ToString(CultureInfo.InvariantCulture) ??
                DateTime.MinValue.ToString(CultureInfo.InvariantCulture),
                PackageValidateDateTime?.ToString(CultureInfo.InvariantCulture) ??
                DateTime.MinValue.ToString(CultureInfo.InvariantCulture));

            return builder.ToString();
        }
    }
}