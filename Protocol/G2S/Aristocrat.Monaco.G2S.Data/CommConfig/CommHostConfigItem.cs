namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using System.Collections.Generic;
    using Common.Storage;

    /// <summary>
    ///     Represents record from CommHostConfigItem data table.
    /// </summary>
    public class CommHostConfigItem : BaseEntity
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfigItem" /> class.
        /// </summary>
        public CommHostConfigItem()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfigItem" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public CommHostConfigItem(long id)
        {
            Id = id;
        }

        /// <summary>
        ///     Gets or sets host index.
        /// </summary>
        public int HostIndex { get; set; }

        /// <summary>
        ///     Gets or sets host id.
        /// </summary>
        public int HostId { get; set; }

        /// <summary>
        ///     Gets or sets host location.
        /// </summary>
        public string HostLocation { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether host registered or not.
        /// </summary>
        public bool HostRegistered { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether default config should be used or not.
        /// </summary>
        public bool UseDefaultConfig { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this required for play or not.
        /// </summary>
        public bool RequiredForPlay { get; set; }

        /// <summary>
        ///     Gets or sets time to live.
        /// </summary>
        public int TimeToLive { get; set; }

        /// <summary>
        ///     Gets or sets no response time.
        /// </summary>
        public int NoResponseTimer { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether multicast is allowed or not.
        /// </summary>
        public bool AllowMulticast { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can modify locally or not.
        /// </summary>
        public bool CanModLocal { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether fault should be displayed or not.
        /// </summary>
        public bool DisplayCommFault { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can modify remote or not.
        /// </summary>
        public bool CanModRemote { get; set; }

        /// <summary>
        ///     Gets or sets the comm host configuration identifier.
        /// </summary>
        /// <value>
        ///     The comm host configuration identifier.
        /// </value>
        public long CommHostConfigId { get; set; }

        /// <summary>
        ///     Gets or sets the comm host configuration devices.
        /// </summary>
        /// <value>
        ///     The comm host configuration devices.
        /// </value>
        public virtual ICollection<CommHostConfigDevice> CommHostConfigDevices { get; set; }
    }
}