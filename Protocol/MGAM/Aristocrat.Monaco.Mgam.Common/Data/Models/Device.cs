namespace Aristocrat.Monaco.Mgam.Common.Data.Models
{
    using System;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Model for the Device.
    /// </summary>
    public class Device : BaseEntity
    {
        /// <summary>
        ///     Gets or sets the device's unique identifier to register with the site-controller.
        /// </summary>
        public Guid DeviceGuid { get; set; }

        /// <summary>
        ///     Gets or sets the device name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the manufacturer name.
        /// </summary>
        public string ManufacturerName { get; set; }
    }
}
