namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Data.OptionConfig.ChangeOptionConfig;
    using ExpressMapper;

    /// <summary>
    ///     Option current value extensions
    /// </summary>
    public static class G2SOptionCurrentValueExtensions
    {
        /// <summary>
        ///     Creates from G2S item.
        /// </summary>
        /// <param name="optionCurrentValues">The option current value.</param>
        /// <returns>Option current value model</returns>
        public static OptionCurrentValue[] CreateOptionCurrentValueFromG2SItem(
            this c_optionCurrentValues optionCurrentValues)
        {
            if (optionCurrentValues.Items == null || optionCurrentValues.Items.Length == 0)
            {
                return new OptionCurrentValue[0];
            }

            return optionCurrentValues.Items.Select(CreateOptionCurrentValue).ToArray();
        }

        /// <summary>
        ///     Creates the option parameter from g2 s item.
        /// </summary>
        /// <param name="optionCurrentValues">The option current values.</param>
        /// <returns>Option parameter model</returns>
        public static OptionParameterDescriptor CreateOptionParameterFromG2SItem(
            this c_optionCurrentValues optionCurrentValues)
        {
            return optionCurrentValues.Item() != null ? CreateOptionParameter(optionCurrentValues.Item()) : null;
        }

        /// <summary>
        ///     Creates the g2s option current value.
        /// </summary>
        /// <param name="optionConfigValue">The option configuration value.</param>
        /// <returns>G2S optionCurrentValues</returns>
        public static optionCurrentValues CreateG2SOptionCurrentValue(this OptionConfigValue optionConfigValue)
        {
            return new optionCurrentValues { Items = new[] { CreateG2SOptionValueItem(optionConfigValue) } };
        }

        /// <summary>
        ///     Creates the g2s option default value.
        /// </summary>
        /// <param name="optionConfigValue">The option configuration value.</param>
        /// <returns>G2S optionDefaultValues</returns>
        public static optionDefaultValues CreateG2SOptionDefaultValue(this OptionConfigValue optionConfigValue)
        {
            return new optionDefaultValues { Items = new[] { CreateG2SOptionValueItem(optionConfigValue) } };
        }

        /// <summary>
        ///     Adds sub value to option config complex value.
        /// </summary>
        /// <param name="value">Complex value</param>
        /// <param name="subValue">Value to add</param>
        /// <returns>Complex value with added sub value</returns>
        public static OptionConfigValue AddSubValue(this OptionConfigValue value, OptionConfigValue subValue)
        {
            if (value.Items == null)
            {
                value.Items = new List<OptionConfigValue>();
            }

            value.Items = value.Items.Concat(new[] { subValue });

            return value;
        }

        /// <summary>
        ///     Single item from option values.
        /// </summary>
        /// <param name="values">The option values.</param>
        /// <returns>Single item.</returns>
        public static object Item(this c_optionCurrentValues values)
        {
            return values.Items != null && values.Items.Length > 0 ? values.Items[0] : null;
        }

        /// <summary>
        ///     Single item from option values.
        /// </summary>
        /// <param name="values">The option values.</param>
        /// <returns>Single item.</returns>
        public static object Item(this c_optionDefaultValues values)
        {
            return values.Items != null && values.Items.Length > 0 ? values.Items[0] : null;
        }

        private static object CreateG2SOptionValueItem(OptionConfigValue optionConfigValue)
        {
            if (optionConfigValue.ValueType == OptionConfigValueType.Integer)
            {
                return Mapper.Map<OptionConfigValue, integerValue1>(optionConfigValue);
            }

            if (optionConfigValue.ValueType == OptionConfigValueType.Decimal)
            {
                return Mapper.Map<OptionConfigValue, decimalValue1>(optionConfigValue);
            }

            if (optionConfigValue.ValueType == OptionConfigValueType.String)
            {
                return Mapper.Map<OptionConfigValue, stringValue1>(optionConfigValue);
            }

            if (optionConfigValue.ValueType == OptionConfigValueType.Boolean)
            {
                return Mapper.Map<OptionConfigValue, booleanValue1>(optionConfigValue);
            }

            if (optionConfigValue.ValueType == OptionConfigValueType.Complex)
            {
                var complexValue = new complexValue
                {
                    paramId = optionConfigValue.ParameterId,
                    Items = CreateG2SOptionCurrentValues(optionConfigValue)
                };

                return complexValue;
            }

            return null;
        }

        private static object[] CreateG2SOptionCurrentValues(OptionConfigValue optionConfigValue)
        {
            return optionConfigValue.Items.Select(CreateG2SOptionValueItem).ToArray();
        }

        private static OptionParameterDescriptor CreateOptionParameter(object item)
        {
            switch (item)
            {
                case c_integerValue integerValue:
                {
                    var optionParameter = new OptionParameterDescriptor
                    {
                        ParameterId = integerValue.paramId,
                        ParameterType = OptionConfigParameterType.Integer
                    };

                    return optionParameter;
                }

                case c_decimalValue decimalValue:
                {
                    var optionParameter = new OptionParameterDescriptor
                    {
                        ParameterId = decimalValue.paramId,
                        ParameterType = OptionConfigParameterType.Decimal
                    };

                    return optionParameter;
                }

                case c_stringValue stringValue:
                {
                    var optionParameter = new OptionParameterDescriptor
                    {
                        ParameterId = stringValue.paramId,
                        ParameterType = OptionConfigParameterType.String
                    };

                    return optionParameter;
                }

                case c_booleanValue booleanValue:
                {
                    var optionParameter = new OptionParameterDescriptor
                    {
                        ParameterId = booleanValue.paramId,
                        ParameterType = OptionConfigParameterType.Boolean
                    };

                    return optionParameter;
                }

                case c_complexValue complexValue:
                {
                    var optionCurrentValue = new OptionParameterDescriptor
                    {
                        ParameterId = complexValue.paramId,
                        ParameterType = OptionConfigParameterType.Complex,
                        ChildItems = CreateOptionParameters(complexValue.Items)
                    };

                    return optionCurrentValue;
                }

                default:
                    return null;
            }
        }

        private static OptionCurrentValue CreateOptionCurrentValue(object item)
        {
            switch (item)
            {
                case c_integerValue integerValue:
                {
                    var optionCurrentValue = new OptionCurrentValue
                    {
                        ParamId = integerValue.paramId,
                        Value = integerValue.Value.ToString(),
                        ParameterType = OptionConfigParameterType.Integer
                    };

                    return optionCurrentValue;
                }

                case c_decimalValue decimalValue:
                {
                    var optionCurrentValue = new OptionCurrentValue
                    {
                        ParamId = decimalValue.paramId,
                        Value = decimalValue.Value.ToString(),
                        ParameterType = OptionConfigParameterType.Decimal
                    };

                    return optionCurrentValue;
                }

                case c_stringValue stringValue:
                {
                    var optionCurrentValue = new OptionCurrentValue
                    {
                        ParamId = stringValue.paramId,
                        Value = stringValue.Value,
                        ParameterType = OptionConfigParameterType.String
                    };

                    return optionCurrentValue;
                }

                case c_booleanValue booleanValue:
                {
                    var optionCurrentValue = new OptionCurrentValue
                    {
                        ParamId = booleanValue.paramId,
                        Value = booleanValue.Value.ToString(),
                        ParameterType = OptionConfigParameterType.Boolean
                    };

                    return optionCurrentValue;
                }

                case c_complexValue complexValue:
                {
                    var optionCurrentValue = new OptionCurrentValue
                    {
                        ParamId = complexValue.paramId,
                        ParameterType = OptionConfigParameterType.Complex,
                        ChildValues = CreateOptionCurrentValues(complexValue.Items)
                    };

                    return optionCurrentValue;
                }

                default:
                    return null;
            }
        }

        private static IEnumerable<OptionCurrentValue> CreateOptionCurrentValues(object[] items)
        {
            var values = new List<OptionCurrentValue>();
            foreach (var item in items)
            {
                var value = CreateOptionCurrentValue(item);
                values.Add(value);
            }

            return values;
        }

        private static IEnumerable<OptionParameterDescriptor> CreateOptionParameters(object[] items)
        {
            var values = new List<OptionParameterDescriptor>();
            foreach (var item in items)
            {
                var value = CreateOptionParameter(item);
                values.Add(value);
            }

            return values;
        }
    }
}