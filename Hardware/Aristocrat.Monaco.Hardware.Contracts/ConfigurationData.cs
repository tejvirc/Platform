namespace Aristocrat.Monaco.Hardware.Contracts
{
    using SharedDevice;

    /// <summary>
    ///     Provides the base hardware configuration data
    /// </summary>
    public class ConfigurationData
    {
        /// <summary>
        ///     Creates the hardware configuration data
        /// </summary>
        /// <param name="deviceType">The device type for this configuration</param>
        /// <param name="enabled">Whether or not the device is enabled</param>
        /// <param name="make">The make for this configuration</param>
        /// <param name="model">The model for this configuration</param>
        /// <param name="port">The port for this configuration</param>
        public ConfigurationData(DeviceType deviceType, bool enabled, string make, string model, string port)
        {
            DeviceType = deviceType;
            Enabled = enabled;
            Make = make;
            Model = model;
            Port = port;
        }

        /// <summary>
        ///     Gets the device type for this hardware configuration
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        ///     Whether or not this hardware configuration is enabled
        /// </summary>
        public bool Enabled { get; }

        /// <summary>
        ///     Gets the make for this hardware configuration
        /// </summary>
        public string Make { get; }

        /// <summary>
        ///     Gets the model for this hardware configuration
        /// </summary>
        public string Model { get; }

        /// <summary>
        ///     Gets the port for this hardware configuration
        /// </summary>
        public string Port { get; }
    }
}