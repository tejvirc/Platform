namespace Aristocrat.G2S.Client.Devices
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     One row of table data for table options.
    /// </summary>
    public class DeviceOptionTableRow
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceOptionTableRow" /> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public DeviceOptionTableRow(IEnumerable<DeviceOptionConfigValue> values)
        {
            Values = values;
        }

        /// <summary>
        ///     Gets the values.
        /// </summary>
        /// <value>
        ///     The values.
        /// </value>
        public IEnumerable<DeviceOptionConfigValue> Values { get; }

        /// <summary>
        ///     Determines whether the specified parameter identifier has value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>
        ///     <c>true</c> if the specified parameter identifier has value; otherwise, <c>false</c>.
        /// </returns>
        public bool HasValue(string parameterId)
        {
            return Values.Any(x => x.ParameterId == parameterId);
        }

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>Get Device option config value.</returns>
        public DeviceOptionConfigValue GetDeviceOptionConfigValue(string parameterId)
        {
            return Values.First(x => x.ParameterId == parameterId);
        }
    }
}