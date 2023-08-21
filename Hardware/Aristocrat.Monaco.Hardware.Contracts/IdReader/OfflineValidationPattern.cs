namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    /// <summary>
    ///     An offline validation pattern for the ID Reader device.
    /// </summary>
    public class OfflineValidationPattern
    {
        /// <summary>
        ///     Initialization of the <see cref="OfflineValidationPattern"/> class.
        ///     Comparable to the PrintableRegion class.
        ///     Is a copy of the Aristocrat.G2S.Protocol.v21 idTypeProfile
        ///     function that can now handle data received in Hardware.
        /// </summary>
        /// <param name="idType">The type of ID the offline pattern applies to.</param>
        /// <param name="offlinePattern">Expected pattern for the ID type.</param>
        public OfflineValidationPattern(
            string idType,
            string offlinePattern
            )
        {
            IdType = idType;
            OfflinePattern = offlinePattern;
        }

        /// <summary>
        ///     The type of ID the offline pattern applies to.
        /// </summary>
        public string IdType { get; set; }

        /// <summary>
        ///     Expected pattern for the ID type.
        /// </summary>
        public string OfflinePattern { get; set; }

    }
}
