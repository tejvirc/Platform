namespace Aristocrat.Monaco.Application.UI.Models
{
    /// <summary>Class to be used in a dictionary of device configurations.</summary>
    public class DeviceConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceConfiguration" /> class.
        /// </summary>
        /// <param name="enabled">Enabled value.</param>
        /// <param name="manufacturer">Manufacturer value.</param>
        /// <param name="protocol">Protocol value.</param>
        /// <param name="port">Port value.</param>
        public DeviceConfiguration(
            bool enabled,
            string manufacturer,
            string protocol,
            int port)
        {
            Enabled = enabled;
            Manufacturer = manufacturer;
            Protocol = protocol;
            Port = port;
        }

        /// <summary>Gets or sets a value indicating whether enabled.</summary>
        public bool Enabled { get; set; }

        /// <summary>Gets or sets a value indicating protocol.</summary>
        public string Manufacturer { get; set; }

        /// <summary>Gets or sets a value indicating protocol.</summary>
        public string Protocol { get; set; }

        /// <summary>Gets or sets a value indicating port.</summary>
        public int Port { get; set; }
    }
}