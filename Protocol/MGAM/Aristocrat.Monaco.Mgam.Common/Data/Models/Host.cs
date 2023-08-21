namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using Monaco.Common.Storage;

    /// <summary>
    ///     Model for the Host.
    /// </summary>
    public class Host : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the service name.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        ///     Gets or sets the directory port.
        /// </summary>
        public int DirectoryPort { get; set; }

        /// <summary>
        ///     Gets or sets the ICD version.
        /// </summary>
        public int IcdVersion { get; set; }

        /// <summary>
        ///   Gets or sets the IDirectoryIpAddress.
        /// </summary>
        public string DirectoryIpAddress { get; set; }

        /// <summary>
        ///   Gets or sets the UseUdpBroadcasting setting.
        /// </summary>
        public bool UseUdpBroadcasting { get; set; }
    }
}
