namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     Entity that represents package error.
    /// </summary>
    public class PackageError : BaseEntity
    {
        /// <summary>
        ///     Gets or sets package Id.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        ///     Gets or sets error code.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        ///     Gets or sets error message.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}