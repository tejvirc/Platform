namespace Aristocrat.Monaco.G2S.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Data.OptionConfig.ChangeOptionConfig;
    using Handlers.OptionConfig.Builders;
    using Kernel;
    using Monaco.Common.Validation;

    /// <inheritdoc />
    public abstract class BaseDeviceOptions : IDeviceOptions
    {
        private const string ProtocolConventionsStringPattern = "^[A-Z0-9]{3}_[ -~]{1,60}$";

        /// <summary>
        ///     Gets the properties manager.
        /// </summary>
        protected IPropertiesManager PropertiesManager => ServiceManager.GetInstance().GetService<IPropertiesManager>();

        /// <summary>
        ///     Gets the supported options for the device.
        /// </summary>
        protected virtual IEnumerable<string> SupportedOptions => new List<string>();

        /// <summary>
        ///     Gets the supported values.
        /// </summary>
        protected virtual IReadOnlyDictionary<string, OptionDataType> SupportedValues
            => new Dictionary<string, OptionDataType>();

        /// <inheritdoc />
        public abstract bool Matches(DeviceClass deviceClass);

        /// <inheritdoc />
        public void ApplyProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
            device.ApplyOptions(optionConfigValues);
            ApplyAdditionalProperties(device, optionConfigValues);
        }

        /// <inheritdoc />
        public virtual ValidationResult Verify(Option option)
        {
            var errors = new List<ValidationError>();

            if (!SupportedOptions.Contains(option.OptionId))
            {
                errors.Add(
                    new ValidationError(
                        option.OptionId,
                        $"Device class {option.DeviceClass} does not support requested option {option.OptionId}"));
            }

            foreach (var optionValue in option.OptionValues)
            {
                VerifyValue(optionValue, errors);
            }

            return new ValidationResult(!errors.Any(), errors);
        }

        /// <summary>
        ///     Adds the protocol options types.
        /// </summary>
        /// <returns>Defined shared protocol options data types.</returns>
        protected static IEnumerable<Tuple<string, OptionDataType>> AddProtocolOptionsTypes()
        {
            return new List<Tuple<string, OptionDataType>>
            {
                new Tuple<string, OptionDataType>(OptionConstants.ProtocolOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.ConfigurationIdParameterName,
                    OptionDataType.Integer),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.NoResponseTimerParameterName,
                    OptionDataType.Integer)
            };
        }

        /// <summary>
        ///     Adds the protocol options 3 types.
        /// </summary>
        /// <returns>Defined shared protocol options data types.</returns>
        protected static IEnumerable<Tuple<string, OptionDataType>> AddProtocolOptions3Types()
        {
            return new List<Tuple<string, OptionDataType>>
            {
                new Tuple<string, OptionDataType>(OptionConstants.ProtocolAdditionalOptionsId, OptionDataType.Complex),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.ConfigDateTimeParameterName,
                    OptionDataType.String),
                new Tuple<string, OptionDataType>(
                    G2SParametersNames.ConfigCompleteParameterName,
                    OptionDataType.Boolean)
            };
        }

        /// <summary>
        ///     Applies the additional properties.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="optionConfigValues">The option configuration values.</param>
        protected virtual void ApplyAdditionalProperties(IDevice device, DeviceOptionConfigValues optionConfigValues)
        {
        }

        /// <summary>
        ///     Checks the parameters.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="optionConfigValues">The option configuration values.</param>
        protected void CheckParameters(int deviceId, DeviceOptionConfigValues optionConfigValues)
        {
            if (deviceId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            if (optionConfigValues == null)
            {
                throw new ArgumentNullException(nameof(optionConfigValues));
            }
        }

        /// <summary>
        ///     Matches the protocol conventions string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if match.</returns>
        protected bool MatchProtocolConventionsString(string value)
        {
            return Regex.IsMatch(value, ProtocolConventionsStringPattern);
        }

        private void VerifyValue(OptionCurrentValue optionValue, IList<ValidationError> errors)
        {
            if (!SupportedValues.TryGetValue(optionValue.ParamId, out var dataType))
            {
                errors.Add(
                    new ValidationError(
                        optionValue.ParamId,
                        $"{GetType().FullName} does not support requested value {optionValue.ParamId}"));
                return;
            }

            switch (dataType)
            {
                case OptionDataType.Complex:
                    foreach (var childValue in optionValue.ChildValues ?? Enumerable.Empty<OptionCurrentValue>())
                    {
                        VerifyValue(childValue, errors);
                    }

                    break;
                case OptionDataType.Integer:
                    if (!optionValue.Value.IsInt())
                    {
                        errors.Add(
                            new ValidationError(
                                optionValue.ParamId,
                                $"{optionValue.ParamId} is not of integer data type"));
                    }

                    break;
                case OptionDataType.Boolean:
                    if (!optionValue.Value.IsBoolean())
                    {
                        errors.Add(
                            new ValidationError(
                                optionValue.ParamId,
                                $"{optionValue.ParamId} is not of bool data type"));
                    }

                    break;
                case OptionDataType.Decimal:
                    if (!optionValue.Value.IsDecimal())
                    {
                        errors.Add(
                            new ValidationError(
                                optionValue.ParamId,
                                $"{optionValue.ParamId} is not of decimal data type"));
                    }

                    break;
                case OptionDataType.String:
                    break;
            }
        }
    }
}
