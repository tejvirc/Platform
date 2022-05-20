namespace Aristocrat.Mgam.Client.Messaging
{
    using System;

    /// <summary>
    ///     VLTs can use this broadcast message to locate services on the site controller.
    /// </summary>
    public class RegisterInstance : Request
    {
        /// <summary>
        ///     Gets or sets the name of the manufacturer of the device.
        /// </summary>
        public string ManufacturerName { get; set; }

        /// <summary>
        ///     Gets or sets the 128-bit unique identifier for the device.
        /// </summary>
        public Guid DeviceGuid { get; set; }

        /// <summary>
        ///     Gets or sets the cabinet serial number.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        ///     Gets or sets the 128-bit unique identifier for the application installation
        ///     instance.
        /// </summary>
        public Guid InstallationGuid { get; set; }

        /// <summary>
        ///     Gets or sets the manufacturer-defined name that corresponds to the
        ///     installation of the application.
        /// </summary>
        public string InstallationName { get; set; }

        /// <summary>
        ///     Gets or sets the 128-bit unique identifier for the applications installed on the
        ///     device.
        /// </summary>
        public Guid ApplicationGuid { get; set; }

        /// <summary>
        ///     Gets or sets the manufacturer-defined name for the application running on the
        ///     device.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Gets or sets the version of the ICD that the VGM is expecting to use.
        /// </summary>
        public int IcdVersion { get; set; } = 1;
    }
}
