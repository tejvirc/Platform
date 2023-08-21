namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Represents option value that can support either primitive values or table values.
    /// </summary>
    public class DeviceOptionConfigValue
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceOptionConfigValue" /> class.
        /// </summary>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <param name="value">The value.</param>
        public DeviceOptionConfigValue(string parameterId, string value)
        {
            ParameterId = parameterId;
            Value = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceOptionConfigValue" /> class.
        /// </summary>
        /// <param name="tableValues">The table values.</param>
        public DeviceOptionConfigValue(IEnumerable<DeviceOptionTableRow> tableValues)
        {
            Table = tableValues;
        }

        /// <summary>
        ///     Gets a value indicating whether this option value is table value.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this value is table; otherwise, <c>false</c>.
        /// </value>
        public bool IsTable => Table != null;

        /// <summary>
        ///     Gets the values count.
        /// </summary>
        /// <value>
        ///     The values count.
        /// </value>
        public int ValuesCount => Table?.Count() ?? 1;

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public string Value { get; }

        /// <summary>
        ///     Gets the parameter identifier.
        /// </summary>
        /// <value>
        ///     The parameter identifier.
        /// </value>
        public string ParameterId { get; }

        /// <summary>
        ///     Gets or sets the table.
        /// </summary>
        /// <value>
        ///     The table.
        /// </value>
        internal IEnumerable<DeviceOptionTableRow> Table { get; set; }

        /// <summary>
        ///     Int32s the value.
        /// </summary>
        /// <returns>Int value.</returns>
        public int Int32Value()
        {
            return GetSimpleValue(val => Convert.ToInt32(Value));
        }

        /// <summary>
        ///     Strings the value.
        /// </summary>
        /// <returns>String value.</returns>
        public string StringValue()
        {
            return GetSimpleValue(val => Value);
        }

        /// <summary>
        ///     Times the span value.
        /// </summary>
        /// <returns>Time span value.</returns>
        public TimeSpan TimeSpanValue()
        {
            return GetSimpleValue(val => TimeSpan.FromMilliseconds(Convert.ToDouble(Value)));
        }

        /// <summary>
        ///     Dates the time value.
        /// </summary>
        /// <returns>Date time value.</returns>
        public DateTime DateTimeValue()
        {
            return GetSimpleValue(val => Convert.ToDateTime(Value));
        }

        /// <summary>
        ///     Conversion to a boolean
        /// </summary>
        /// <returns>Bool value.</returns>
        public bool BooleanValue()
        {
            return GetSimpleValue(val => Convert.ToBoolean(Value));
        }

        /// <summary>
        ///     Longs the value.
        /// </summary>
        /// <returns>Long value.</returns>
        public long Int64Value()
        {
            return GetSimpleValue(val => Convert.ToInt64(Value));
        }

        private T GetSimpleValue<T>(Func<string, T> getterFunc)
        {
            if (IsTable)
            {
                throw new InvalidOperationException("Accessing table value as simple property");
            }

            return getterFunc(Value);
        }
    }
}