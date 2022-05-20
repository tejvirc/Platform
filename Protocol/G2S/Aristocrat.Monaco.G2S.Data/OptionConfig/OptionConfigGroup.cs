namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using Common.Storage;

    /// <summary>
    ///     Represents record from OptionConfigGroup data table.
    /// </summary>
    public class OptionConfigGroup : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the option group identifier.
        /// </summary>
        /// <value>
        ///     The option group identifier.
        /// </value>
        public string OptionGroupId { get; set; }

        /// <summary>
        ///     Gets or sets the name of the option group.
        /// </summary>
        /// <value>
        ///     The name of the option group.
        /// </value>
        public string OptionGroupName { get; set; }

        /// <summary>
        ///     Gets or sets the option configuration group identifier.
        /// </summary>
        /// <value>
        ///     The option configuration group identifier.
        /// </value>
        public long OptionConfigDeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the option configuration device.
        /// </summary>
        /// <value>
        ///     The option configuration device.
        /// </value>
        [ForeignKey("OptionConfigDeviceId")]
        public virtual OptionConfigDeviceEntity OptionConfigDevice { get; set; }

        /// <summary>
        ///     Gets or sets the option configuration items.
        /// </summary>
        /// <value>
        ///     The option configuration items.
        /// </value>
        public virtual ICollection<OptionConfigItem> OptionConfigItems { get; set; }
    }
}