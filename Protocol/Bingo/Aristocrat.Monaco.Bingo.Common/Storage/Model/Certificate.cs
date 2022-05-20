namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     Model for a certificate.
    /// </summary>
    public class Certificate : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the certificate thumbprint.
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        ///     Get or set the certificate's raw data.
        /// </summary>
        public byte[] RawData { get; set; }
    }
}
