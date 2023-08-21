namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Option config values
    /// </summary>
    public class DeviceOptionConfigValues
    {
        private readonly Dictionary<string, DeviceOptionConfigValue> _options =
            new Dictionary<string, DeviceOptionConfigValue>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceOptionConfigValues" /> class.
        /// </summary>
        /// <param name="configurationId">The configuration identifier.</param>
        public DeviceOptionConfigValues(long configurationId)
        {
            ConfigurationId = configurationId;
        }

        /// <summary>
        ///     Gets the configuration identifier.
        /// </summary>
        /// <value>
        ///     The configuration identifier.
        /// </value>
        public long ConfigurationId { get; }

        /// <summary>
        ///     Adds the option.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <param name="optionValue">The parameter value.</param>
        public void AddOption(string parameterId, DeviceOptionConfigValue optionValue)
        {
            _options[parameterId] = optionValue;
        }

        /// <summary>
        ///     Adds the option.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <param name="optionValue">The parameter value.</param>
        public void AddOption(string parameterId, string optionValue)
        {
            _options[parameterId] = new DeviceOptionConfigValue(parameterId, optionValue);
        }

        /// <summary>
        ///     Determines whether the specified parameter identifier has value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>
        ///     <c>true</c> if the specified parameter identifier has value; otherwise, <c>false</c>.
        /// </returns>
        public bool HasValue(string parameterId)
        {
            return _options.ContainsKey(parameterId) && _options[parameterId].Value != null;
        }

        /// <summary>
        ///     Determines whether the specified parameter identifier key is present.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>
        ///     <c>true</c> if the specified parameter identifier key is present; otherwise, <c>false</c>.
        /// </returns>
        public bool HasKey(string parameterId)
        {
            return _options.ContainsKey(parameterId);
        }

        /// <summary>
        ///     Determines whether the specified parameter identifier has a table value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>
        ///     <c>true</c> if the specified parameter identifier has a table value; otherwise, <c>false</c>.
        /// </returns>
        public bool HasTableValue(string parameterId)
        {
            return _options.ContainsKey(parameterId) && _options[parameterId].Table != null;
        }

        /// <summary>
        ///     Longs the value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>Long value.</returns>
        public long Int64Value(string parameterId)
        {
            return EnsureGetSimpleValue(parameterId).Int64Value();
        }

        /// <summary>
        ///     Converts the value to a boolean
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>Bool value.</returns>
        public bool BooleanValue(string parameterId)
        {
            return EnsureGetSimpleValue(parameterId).BooleanValue();
        }

        /// <summary>
        ///     Dates the time value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>Date time value.</returns>
        public DateTime DateTimeValue(string parameterId)
        {
            return EnsureGetSimpleValue(parameterId).DateTimeValue();
        }

        /// <summary>
        ///     Times the span value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>Time span value.</returns>
        public TimeSpan TimeSpanValue(string parameterId)
        {
            return EnsureGetSimpleValue(parameterId).TimeSpanValue();
        }

        /// <summary>
        ///     Int32s the value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>Int value.</returns>
        public int Int32Value(string parameterId)
        {
            return EnsureGetSimpleValue(parameterId).Int32Value();
        }

        /// <summary>
        ///     Strings the value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>String value.</returns>
        public string StringValue(string parameterId)
        {
            return EnsureGetSimpleValue(parameterId).StringValue();
        }

        /// <summary>
        ///     Gets the table value.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <returns>List of DeviceOptionTableRow</returns>
        public IEnumerable<DeviceOptionTableRow> GetTableValue(string parameterId)
        {
            var val = _options[parameterId];

            if (!val.IsTable)
            {
                throw new InvalidOperationException("Accessing simple value as table property");
            }

            return val.Table;
        }

        private DeviceOptionConfigValue EnsureGetSimpleValue(string parameterId)
        {
            var val = _options[parameterId];
            if (val.IsTable)
            {
                throw new InvalidOperationException("Accessing table value as simple property");
            }

            return val;
        }
    }
}