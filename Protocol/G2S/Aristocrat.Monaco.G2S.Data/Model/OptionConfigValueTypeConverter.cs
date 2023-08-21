namespace Aristocrat.Monaco.G2S.Data.Model
{
    using System;

    /// <summary>
    ///     Provides the ability to convert value into the type,
    ///     provided by <see cref="OptionConfigValueType" />
    /// </summary>
    public static class OptionConfigValueTypeConverter
    {
        /// <summary>
        ///     Converts value into specific type.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="type">Type</param>
        /// <returns>Converted value</returns>
        public static object Convert(string value, OptionConfigValueType type)
        {
            switch (type)
            {
                case OptionConfigValueType.Integer:
                    return !long.TryParse(value, out var longResult) ? default(long) : longResult;
                case OptionConfigValueType.Decimal:
                    return !decimal.TryParse(value, out _) ? default(decimal) : System.Convert.ToDecimal(value);
                case OptionConfigValueType.Boolean:
                    return bool.TryParse(value, out var booleanResult) && booleanResult;
                case OptionConfigValueType.String:
                    return value;
                case OptionConfigValueType.Complex:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException(@"Invalid OptionConfigValueType specified", nameof(type));
            }
        }
    }
}