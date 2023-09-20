namespace Aristocrat.Monaco.G2S.Data.Model
{
    using System;
    using System.Globalization;
    using System.Text;
    using Common.Storage;

    /// <summary>
    ///     Package Activity type
    /// </summary>
    public enum PackageActivityType
    {
        /// <summary>
        ///     Other
        /// </summary>
        Other,

        /// <summary>
        ///     Create package
        /// </summary>
        Create,

        /// <summary>
        ///     Delete package
        /// </summary>
        Delete
    }

    /// <summary>
    ///     Print Log Entity
    /// </summary>
    public class PackageLog : BaseEntity, ILogSequence
    {
        /// <summary>
        ///     Gets or sets Package Id
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        ///     Gets or sets get/sets package size.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        ///     Gets or sets reason of code.
        /// </summary>
        public string ReasonCode { get; set; }

        /// <summary>
        ///     Gets or sets transfer error code.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        ///     Gets or sets package state.
        /// </summary>
        public PackageState State { get; set; }

        /// <summary>
        ///     Gets or sets package exception.
        /// </summary>
        public int Exception { get; set; }

        /// <summary>
        ///     Gets or sets device Id
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets transaction id.
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets activityDateTime.
        /// </summary>
        public DateTimeOffset? ActivityDateTime { get; set; }

        /// <summary>
        ///     Gets or sets ActivityType.
        /// </summary>
        public PackageActivityType? ActivityType { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to overwrite.
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        ///     Gets or sets the hash of the package (256 bytes)
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        ///     Gets or sets transfer unique identifier.
        /// </summary>
        public int TransferId { get; set; }

        /// <summary>
        ///     Gets or sets transfer location that is an IP address:port/path/filename or valid network URI address.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        ///     Gets or sets optional parameters required for the transfer of the package. Could be empty.
        /// </summary>
        public string Parameters { get; set; }
        /// <summary>
        ///     Gets or sets transfer state.
        /// </summary>
        public TransferState TransferState { get; set; }

        /// <summary>
        ///     Gets or sets transfer type.
        /// </summary>
        public TransferType TransferType { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to delete the package after transfer completes.
        /// </summary>
        public bool DeleteAfter { get; set; }

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
        public DateTimeOffset? TransferCompletedDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the package validate datetime.
        /// </summary>
        public DateTimeOffset? PackageValidateDateTime { get; set; }

        /// <summary>
        ///     Returns a human-readable representation of a Package.
        /// </summary>
        /// <returns>A human-readable string.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0} [PackageId={1}, DeviceId={2}, State={3}, Size={4}, Exception={5}, ActivityDateTime={6}, ActivityType={7}, Overwrite={8}, Hash={9}, TransactionId={10}, ReasonCode={11}",
                GetType(),
                PackageId,
                DeviceId,
                State,
                Size,
                Exception,
                ActivityDateTime?.ToString(CultureInfo.InvariantCulture) ??
                DateTime.MinValue.ToString(CultureInfo.InvariantCulture),
                ActivityType ?? PackageActivityType.Other,
                Overwrite,
                Hash,
                TransactionId,
                ReasonCode);

            return builder.ToString();
        }

        /// <summary>
        ///     Clones a package log.
        /// </summary>
        /// <returns>Returns a new PackageLog.</returns>
        public PackageLog CreateActivityLog()
        {
            return new PackageLog() {
                PackageId = PackageId,
                ActivityDateTime = ActivityDateTime,
                ActivityType = ActivityType,
                DeviceId = DeviceId,
                ErrorCode = ErrorCode,
                Exception = Exception,
                Hash = Hash,
                Overwrite = Overwrite,
                ReasonCode = ReasonCode,
                State = State,
                Size = Size,
                TransactionId = TransactionId
            };
        }
    }
}