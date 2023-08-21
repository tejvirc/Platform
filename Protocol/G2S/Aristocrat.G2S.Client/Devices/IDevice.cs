namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Contract for a client device
    /// </summary>
    public interface IDevice : IRequiredForPlay, IDeviceControl
    {
        /// <summary>
        ///     Gets the unique device id
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether indicates whether the device has been enabled or disabled by the EGM.
        ///     (true = enabled, false = disabled)
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether indicates whether the device has been enabled or disabled by the host.
        ///     (true = enabled, false = disabled)
        /// </summary>
        bool HostEnabled { get; set; }

        /// <summary>
        ///     Gets configuration identifier set by the configuration host
        /// </summary>
        long ConfigurationId { get; }

        /// <summary>
        ///     Gets the device owner
        /// </summary>
        int Owner { get; }

        /// <summary>
        ///     Gets configuration identifier set by the configuration host
        /// </summary>
        int Configurator { get; }

        /// <summary>
        ///     Gets the guest list
        /// </summary>
        IEnumerable<int> Guests { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether indicates whether the EGM has been locked by the device
        /// </summary>
        bool Locked { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether indicates whether the device has been locked by the host
        /// </summary>
        bool HostLocked { get; set; }

        /// <summary>
        ///     Gets a value indicating whether true is this device is active
        /// </summary>
        bool Active { get; }

        /// <summary>
        ///     Gets a value indicating whether indicates whether the default configuration for the device MUST be used when the
        ///     EGM restarts.
        /// </summary>
        bool UseDefaultConfig { get; }

        /// <summary>
        ///     Gets the device class name
        /// </summary>
        string DeviceClass { get; }

        /// <summary>
        ///     Gets the device class prefix
        /// </summary>
        string DevicePrefix { get; }

        /// <summary>
        ///     Gets the date and time that the configuration of the device was last changed.
        /// </summary>
        DateTime ConfigDateTime { get; }

        /// <summary>
        ///     Gets a value indicating whether the configuration of the device is complete.
        /// </summary>
        bool ConfigComplete { get; }

        /// <summary>
        ///     Gets the Date/time that the subscription or configuration information set by this device was last updated by the
        ///     device or by another mechanism.
        /// </summary>
        DateTime? ListStateDateTime { get; }

        /// <summary>
        ///     Gets a value indicating whether the device is an existing device.  If true the device was loaded from persistent
        ///     storage, otherwise false
        /// </summary>
        bool Existing { get; }

        /// <summary>
        ///     Applies the options.
        /// </summary>
        /// <param name="optionConfigValues">The option configuration values.</param>
        void ApplyOptions(DeviceOptionConfigValues optionConfigValues);
    }
}