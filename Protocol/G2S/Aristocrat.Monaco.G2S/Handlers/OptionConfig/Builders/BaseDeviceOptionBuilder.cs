namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;

    /// <summary>
    ///     Base device options builder.
    /// </summary>
    /// <typeparam name="TDevice">The type of the device.</typeparam>
    /// <seealso cref="IDeviceOptionsBuilder" />
    public abstract class BaseDeviceOptionBuilder<TDevice> : IDeviceOptionsBuilder
        where TDevice : IDevice
    {
        /// <summary>
        ///     Gets the device class.
        /// </summary>
        /// <value>
        ///     The device class.
        /// </value>
        protected abstract DeviceClass DeviceClass { get; }

        /// <inheritdoc />
        public bool Matches(DeviceClass deviceClass)
        {
            return deviceClass == DeviceClass;
        }

        /// <inheritdoc />
        public deviceOptions Build(IDevice device, OptionListCommandBuilderParameters parameters)
        {
            return new deviceOptions
            {
                deviceClass = device.PrefixedDeviceClass(),
                deviceId = device.Id,
                optionGroup = BuildGroups((TDevice)device, parameters)
            };
        }

        /// <summary>
        ///     Builds the option groups with items for device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Array of groups.</returns>
        protected abstract optionGroup[] BuildGroups(TDevice device, OptionListCommandBuilderParameters parameters);

        /// <summary>
        ///     Defines whether we should include the parameter.
        /// </summary>
        /// <param name="parameterName">The parameter to consider.</param>
        /// <param name="parameters">The filter parameters.</param>
        /// <returns>Result</returns>
        protected bool ShouldIncludeParam(string parameterName, OptionListCommandBuilderParameters parameters)
        {
            return parameters.IncludeAllOptions || parameterName.Equals(parameters.OptionId);
        }

        /// <summary>
        ///     Builds the option item.
        /// </summary>
        /// <param name="optionId">The option identifier.</param>
        /// <param name="level">The level.</param>
        /// <param name="minSelections">The minimum selections.</param>
        /// <param name="maxSelections">The maximum selections.</param>
        /// <param name="paramId">The parameter identifier.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="paramHelp">The parameter help.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="includeDetails">true to include details</param>
        /// <returns>Option item.</returns>
        protected optionItem BuildOptionItem(
            string optionId,
            t_securityLevels level,
            int minSelections,
            int maxSelections,
            string paramId,
            string paramName,
            string paramHelp,
            IEnumerable<ParameterDescription> parameters,
            bool includeDetails)
        {
            var optionParameters = parameters.Select(
                p =>
                {
                    var param = p.ParamCreator();
                    param.paramId = p.ParamId;
                    param.paramName = p.ParamName;
                    param.paramHelp = p.ParamHelp;
                    return (object)param;
                }).ToArray();

            var optionValues = parameters.Select(
                p =>
                {
                    var val = p.ValueCreator();
                    val.paramId = p.ParamId;
                    val.Value = p.Value;
                    return (object)val;
                }).ToArray();

            var optionDefaultValues = parameters.Select(
                p =>
                {
                    var val = p.ValueCreator();
                    val.paramId = p.ParamId;
                    val.Value = p.DefaultValue;
                    return (object)val;
                }).ToArray();

            return new optionItem
            {
                optionId = optionId,
                securityLevel = level,
                minSelections = minSelections,
                maxSelections = maxSelections,
                optionParameters = includeDetails
                    ? new optionParameters
                    {
                        Item = new complexParameter
                        {
                            paramId = paramId,
                            paramName = paramName,
                            paramHelp = paramHelp,
                            Items = optionParameters
                        }
                    }
                    : null,
                optionCurrentValues = new optionCurrentValues
                {
                    Items = new object[]
                    {
                        new complexValue
                        {
                            paramId = paramId,
                            Items = optionValues
                        }
                    }
                },
                optionDefaultValues = includeDetails
                    ? new optionDefaultValues
                    {
                        Items = new object[]
                        {
                            new complexValue
                            {
                                paramId = paramId,
                                Items = optionDefaultValues
                            }
                        }
                    }
                    : null
            };
        }

        /// <summary>
        ///     Builds the option item with table values.
        /// </summary>
        /// <param name="optionId">The option identifier.</param>
        /// <param name="level">The level.</param>
        /// <param name="minSelections">The minimum selections.</param>
        /// <param name="maxSelections">The maximum selections.</param>
        /// <param name="parameterId">The parameter identifier.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterHelp">The parameter help.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="currentValues">Table of values.</param>
        /// <param name="defaultValues">List of currentValues</param>
        /// <param name="includeDetails">true to include details</param>
        /// <returns>Option item.</returns>
        protected optionItem BuildOptionItemForTable(
            string optionId,
            t_securityLevels level,
            int minSelections,
            int maxSelections,
            string parameterId,
            string parameterName,
            string parameterHelp,
            IEnumerable<ParameterDescription> parameters,
            IEnumerable<complexValue> currentValues,
            IEnumerable<complexValue> defaultValues,
            bool includeDetails)
        {
            var optionParameters = parameters.Select(
                p =>
                {
                    var param = p.ParamCreator();
                    param.paramId = p.ParamId;
                    param.paramName = p.ParamName;
                    param.paramHelp = p.ParamHelp;
                    return param;
                }).ToArray();
            

            return new optionItem
            {
                optionId = optionId,
                securityLevel = level,
                minSelections = minSelections,
                maxSelections = maxSelections == 0 ? 1 : maxSelections,
                optionParameters = includeDetails
                    ? new optionParameters
                    {
                        Item = new complexParameter
                        {
                            paramId = parameterId,
                            paramName = parameterName,
                            paramHelp = parameterHelp,
                            Items = optionParameters
                        }
                    }
                    : null,
                optionCurrentValues =
                    currentValues.Any()
                        ? new optionCurrentValues { Items = currentValues.ToArray() }
                        : new optionCurrentValues(),
                optionDefaultValues =
                    defaultValues.Any() && includeDetails
                        ? new optionDefaultValues { Items = defaultValues.ToArray() }
                        : null
            };
        }


        protected optionItem BuildOptionItemOfNonComplexType(
            string optionId,
            t_securityLevels level,
            int minSelections,
            int maxSelections,
            string paramId,
            string paramName,
            string paramHelp,
            ParameterDescription parameter,
            bool includeDetails)
        {
            var optionParameter = parameter.ParamCreator();
            optionParameter.paramId = parameter.ParamId;
            optionParameter.paramName = parameter.ParamName;
            optionParameter.paramHelp = parameter.ParamHelp;

            var optionValue = parameter.ValueCreator();
            optionValue.paramId = parameter.ParamId;
            optionValue.Value = parameter.Value;

            var optionDefaultValue = parameter.ValueCreator();
            optionDefaultValue.paramId = parameter.ParamId;
            optionDefaultValue.Value = parameter.DefaultValue;

            return new optionItem
            {
                optionId = optionId,
                securityLevel = level,
                minSelections = minSelections,
                maxSelections = maxSelections,
                optionParameters = includeDetails ? new optionParameters { Item = optionParameter } : null,
                optionCurrentValues = new optionCurrentValues { Items = new[] { optionValue } },
                optionDefaultValues =
                    includeDetails ? new optionDefaultValues { Items = new[] { optionDefaultValue } } : null
            };
        }

        /// <summary>
        ///     Description of the configId parameter.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Description of parameter structure.</returns>
        protected ParameterDescription ConfigIdParameter(long id)
        {
            return new ParameterDescription
            {
                ParamId = G2SParametersNames.ConfigurationIdParameterName,
                ParamName = "Configuration Identifier",
                ParamHelp = "ID assigned by the last successful G2S option configuration",
                ParamCreator = () => new integerParameter(),
                ValueCreator = () => new integerValue1(),
                Value = id,
                DefaultValue = 0
            };
        }

        /// <summary>
        ///     Description of the useDefaultConfig parameter.
        /// </summary>
        /// <param name="useDefaultConfig">The use default config value.</param>
        /// <returns>Description of parameter structure.</returns>
        protected ParameterDescription UseDefaultConfigParameter(bool useDefaultConfig)
        {
            return new ParameterDescription
            {
                ParamId = G2SParametersNames.UseDefaultConfigParameterName,
                ParamName = "Use Default Configuration",
                ParamHelp = "Use default configuration on restart",
                ParamCreator = () => new booleanParameter(),
                ValueCreator = () => new booleanValue1(),
                Value = useDefaultConfig,
                DefaultValue = false
            };
        }

        /// <summary>
        ///     Description of the noResponseTimer parameter.
        /// </summary>
        /// <param name="time">Time for no response param.</param>
        /// <param name="helpText">Help text to return for param.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Description of parameter structure.</returns>
        protected ParameterDescription NoResponseParameter(TimeSpan time, string helpText, int defaultValue)
        {
            return new ParameterDescription
            {
                ParamId = G2SParametersNames.NoResponseTimerParameterName,
                ParamName = "No Response Timer",
                ParamHelp = helpText,
                ParamCreator = () => new integerParameter { canModRemote = true },
                ValueCreator = () => new integerValue1(),
                Value = (int)time.TotalMilliseconds,
                DefaultValue = defaultValue
            };
        }

        /// <summary>
        ///     Builds the protocol options.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="restartStatus">Restart status.</param>
        /// <param name="itemHelpText">Device specific help text for protocol options item.</param>
        /// <param name="includeDetails">true to include details</param>
        /// <returns>g2s option item.</returns>
        protected optionItem BuildProtocolOptions(
            TDevice device,
            bool restartStatus,
            string itemHelpText,
            bool includeDetails)
        {
            return BuildProtocolOptions(device, restartStatus, itemHelpText, null, includeDetails);
        }

        /// <summary>
        ///     Builds the protocol options.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="restartStatus">Restart status.</param>
        /// <param name="itemHelpText">Device specific help text for protocol options item.</param>
        /// <param name="additionalParameters">Additional parameters to include.</param>
        /// <param name="includeDetails">true to include details</param>
        /// <returns>g2s option item.</returns>
        protected optionItem BuildProtocolOptions(
            TDevice device,
            bool restartStatus,
            string itemHelpText,
            IEnumerable<ParameterDescription> additionalParameters,
            bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                ConfigIdParameter(device.ConfigurationId),
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.RestartStatusParameterName,
                    ParamName = "Enabled on Restart",
                    ParamHelp = "Controls hostEnabled attribute status upon EGM restart",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = restartStatus,
                    DefaultValue = true
                },
                UseDefaultConfigParameter(device.UseDefaultConfig),
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.RequiredForPlayParameterName,
                    ParamName = "Required For Play",
                    ParamHelp = "Device is required for game play",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.RequiredForPlay,
                    DefaultValue = false
                }
            };

            if (additionalParameters != null)
            {
                parameters.AddRange(additionalParameters);
            }

            return BuildOptionItem(
                OptionConstants.ProtocolOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_protocolParams",
                "G2S Protocol Parameters",
                itemHelpText,
                parameters,
                includeDetails);
        }

        /// <summary>
        ///     Builds the protocol additional options (protocol options 3).
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="includeDetails">true to include details</param>
        /// <returns>g2s option item.</returns>
        protected optionItem BuildProtocolAdditionalOptions(TDevice device, bool includeDetails)
        {
            return BuildProtocolAdditionalOptions(device, null, includeDetails);
        }

        /// <summary>
        ///     Builds the protocol additional options (protocol options 3).
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="additionalParameters">Additional parameters to include.</param>
        /// <param name="includeDetails">true to include details</param>
        /// <returns>g2s option item.</returns>
        protected optionItem BuildProtocolAdditionalOptions(
            TDevice device,
            IEnumerable<ParameterDescription> additionalParameters,
            bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ConfigDateTimeParameterName,
                    ParamName = "Configuration Date/Time",
                    ParamHelp = "Date/time device configuration was last changed.",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    Value = device.ConfigDateTime == DateTime.MinValue
                        ? string.Empty
                        : device.ConfigDateTime.ToUniversalTime().ToString("o"),
                    DefaultValue = string.Empty
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ConfigCompleteParameterName,
                    ParamName = "Configuration Complete",
                    ParamHelp = "Indicates whether the device configuration is complete.",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = device.ConfigComplete,
                    DefaultValue = true
                }
            };

            if (additionalParameters != null)
            {
                parameters.AddRange(additionalParameters);
            }

            return BuildOptionItem(
                OptionConstants.ProtocolAdditionalOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_protocolParams3",
                "Additional G2S Protocol Parameters",
                "More standard G2S protocol parameters for configuration options",
                parameters,
                includeDetails);
        }
    }
}
