namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using System.Collections.Generic;
    using Common.Storage;

    /// <summary>
    ///     Represents record from CommHostConfig data table.
    /// </summary>
    public class CommHostConfig : BaseEntity
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfig" /> class.
        /// </summary>
        public CommHostConfig()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfig" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public CommHostConfig(long id)
        {
            Id = id;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether multicast is supported or not.
        /// </summary>
        public bool? MulticastSupported { get; set; }

        /// <summary>
        ///     Gets or sets the comm host configuration items.
        /// </summary>
        /// <value>
        ///     The comm host configuration items.
        /// </value>
        public virtual ICollection<CommHostConfigItem> CommHostConfigItems { get; set; }
    }
}