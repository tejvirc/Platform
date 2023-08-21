namespace Aristocrat.Monaco.G2S.Data.Extensions
{
    using System;
    using Model;

    /// <summary>
    ///     Extensions to convert G2S string enums to code enums in both directions.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Converts the type of to option configuration value.
        /// </summary>
        /// <param name="optionConfigParameterType">Type of the option configuration parameter.</param>
        /// <returns>Option config value type</returns>
        /// <exception cref="ArgumentException">Not valid value for optionConfigParameterType</exception>
        public static OptionConfigValueType ConvertToOptionConfigValueType(
            this OptionConfigParameterType optionConfigParameterType)
        {
            switch (optionConfigParameterType)
            {
                case OptionConfigParameterType.Integer:
                    return OptionConfigValueType.Integer;
                case OptionConfigParameterType.Decimal:
                    return OptionConfigValueType.Decimal;
                case OptionConfigParameterType.String:
                    return OptionConfigValueType.String;
                case OptionConfigParameterType.Boolean:
                    return OptionConfigValueType.Boolean;
                case OptionConfigParameterType.Complex:
                    return OptionConfigValueType.Complex;
                default:
                    throw new ArgumentException("Not valid value for optionConfigParameterType");
            }
        }
    }
}