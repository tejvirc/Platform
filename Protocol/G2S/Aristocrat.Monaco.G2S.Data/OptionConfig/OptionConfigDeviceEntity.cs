namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.Collections.Generic;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Represents record from OptionConfigDeviceEntity data table.
    /// </summary>
    public class OptionConfigDeviceEntity : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the device class.
        /// </summary>
        /// <value>
        ///     The device class.
        /// </value>
        public DeviceClass DeviceClass { get; set; }

        /// <summary>
        ///     Gets or sets the device identifier.
        /// </summary>
        /// <value>
        ///     The device identifier.
        /// </value>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the option configuration groups.
        /// </summary>
        /// <value>
        ///     The option configuration groups.
        /// </value>
        public virtual ICollection<OptionConfigGroup> OptionConfigGroups { get; set; }
    }
}