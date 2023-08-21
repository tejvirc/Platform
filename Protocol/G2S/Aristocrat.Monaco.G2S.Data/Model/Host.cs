namespace Aristocrat.Monaco.G2S.Data.Model
{
    using Common.Storage;

    /// <summary>
    ///     Defines a Host
    /// </summary>
    public class Host : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the host identifier
        /// </summary>
        public int HostId { get; set; }

        /// <summary>
        ///     Gets or sets the Url of the endpoint
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the host is registered.
        /// </summary>
        public bool Registered { get; set; }

        /// <summary>
        ///     Gets a value indicating whether indicates the device MUST be functioning and enabled before the EGM can be
        ///     played.
        ///     (true = enabled, false = disabled)
        /// </summary>
        public bool RequiredForPlay { get; set; }
    }
}