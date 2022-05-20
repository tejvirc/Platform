namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using ExpressMapper;

    /// <summary>
    ///     G2S optionParameters extensions
    /// </summary>
    public static class G2SOptionConfigParameterExtensions
    {
        /// <summary>
        ///     Creates the g2s option configuration parameter.
        /// </summary>
        /// <param name="optionConfigParameter">The option configuration parameter.</param>
        /// <returns>G2S optionParameters</returns>
        public static optionParameters CreateG2SOptionConfigParameter(this OptionConfigParameter optionConfigParameter)
        {
            return new optionParameters { Item = CreateG2SOptionParameterItem(optionConfigParameter) };
        }

        /// <summary>
        ///     Adds subparameter to option config complex parameter.
        /// </summary>
        /// <param name="parameter">Complex parameter</param>
        /// <param name="subParameter">Parameter to add</param>
        /// <returns>Complex parameter with added subparameter</returns>
        public static OptionConfigComplexParameter AddSubParameter(
            this OptionConfigComplexParameter parameter,
            OptionConfigParameter subParameter)
        {
            if (parameter.Items == null)
            {
                parameter.Items = new List<OptionConfigParameter>();
            }

            parameter.Items = parameter.Items.Concat(new[] { subParameter });

            return parameter;
        }

        private static object CreateG2SOptionParameterItem(OptionConfigParameter optionConfigParameter)
        {
            // TODO: map enum values for parameters as soon as they are added to the lib (see c_integerDataType.integerEnum for example)
            if (optionConfigParameter.ParameterType == OptionConfigParameterType.Integer)
            {
                return
                    Mapper.Map<OptionConfigIntegerParameter, integerParameter>(
                        (OptionConfigIntegerParameter)optionConfigParameter);
            }

            if (optionConfigParameter.ParameterType == OptionConfigParameterType.Decimal)
            {
                return
                    Mapper.Map<OptionConfigDecimalParameter, decimalParameter>(
                        (OptionConfigDecimalParameter)optionConfigParameter);
            }

            if (optionConfigParameter.ParameterType == OptionConfigParameterType.String)
            {
                return
                    Mapper.Map<OptionConfigStringParameter, stringParameter>(
                        (OptionConfigStringParameter)optionConfigParameter);
            }

            if (optionConfigParameter.ParameterType == OptionConfigParameterType.Boolean)
            {
                return
                    Mapper.Map<OptionConfigBooleanParameter, booleanParameter>(
                        (OptionConfigBooleanParameter)optionConfigParameter);
            }

            if (optionConfigParameter.ParameterType == OptionConfigParameterType.Complex)
            {
                var complexParameter =
                    Mapper.Map<OptionConfigComplexParameter, complexParameter>(
                        (OptionConfigComplexParameter)optionConfigParameter);

                complexParameter.Items =
                    CreateG2SOptionParameterItems((OptionConfigComplexParameter)optionConfigParameter);

                return complexParameter;
            }

            return null;
        }

        private static object[] CreateG2SOptionParameterItems(OptionConfigComplexParameter optionConfigParameter)
        {
            return optionConfigParameter.Items.Select(CreateG2SOptionParameterItem).ToArray();
        }
    }
}