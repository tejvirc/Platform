namespace Aristocrat.Monaco.Mgam.Common
{
    using System;

    /// <summary>
    ///     Instance registration information.
    /// </summary>
    public class RegistrationInfo
    {
        /// <summary>
        ///     Gets or sets the manufacturer name.
        /// </summary>
        public string ManufacturerName { get; set; }

        /// <summary>
        ///     Gets or sets the device ID.
        /// </summary>
        public Guid DeviceGuid { get; set; }

        /// <summary>
        ///     Gets or sets the device name.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        ///     Gets or sets the installation ID.
        /// </summary>
        public Guid InstallationGuid { get; set; }

        /// <summary>
        ///     Gets or sets the installation name.
        /// </summary>
        public string InstallationName { get; set; }

        /// <summary>
        ///     Gets or sets the application ID.
        /// </summary>
        public Guid ApplicationGuid { get; set; }

        /// <summary>
        ///     Gets or sets the application name.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Gets or sets the ICD version.
        /// </summary>
        public int IcdVersion { get; set; }
    }
}
