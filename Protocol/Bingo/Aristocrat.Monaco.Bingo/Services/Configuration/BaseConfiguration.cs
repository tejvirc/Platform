namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Common;
    using Common.Exceptions;
    using Common.Storage.Model;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using ServerApiGateway;

    public abstract class BaseConfiguration : IConfiguration
    {
        protected readonly ILog Logger;
        protected readonly IPropertiesManager PropertiesManager;
        protected readonly ISystemDisableManager SystemDisable;

        protected Dictionary<string, (string propertyName, Func<string, object> conversionFunction)> ConfigurationConversion { get; set; }

        protected HashSet<string> RequiredSettings { get; set; }

        protected BaseConfiguration(IPropertiesManager propertiesManager, ISystemDisableManager systemDisableManager)
        {
            PropertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            SystemDisable = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            Logger = LogManager.GetLogger(GetType());
        }

        public void Configure(
            IEnumerable<ConfigurationResponse.Types.ClientAttribute> messageConfigurationAttribute,
            BingoServerSettingsModel model)
        {
            ConfigurationConversion ??= new Dictionary<string, (string propertyName, Func<string, object> conversionFunction)>();
            RequiredSettings ??= new HashSet<string>();

            foreach (var config in messageConfigurationAttribute)
            {
                var name = config.Name;
                var value = config.Value;

                // handle things like PaytableId_0, DenominationId_0, etc.
                var nameParts = name.Split('_');
                if (nameParts.Length > 1 && int.TryParse(nameParts[1], out _))
                {
                    Logger.Info($"Transformed config key '{name}' --> '{nameParts[0]}'");
                    name = nameParts[0];
                }

                AccountForRequiredSettings(name);

                if (IsSettingInvalid(name, value))
                {
                    LogInvalidSetting(name, value, ConfigurationFailureReason.InvalidGameConfiguration);
                    continue;
                }

                if (SettingChangedThatRequiresNvRamClear(name, value, model))
                {
                    LogInvalidSetting(name, value, ConfigurationFailureReason.ConfigurationMismatch);
                    continue;
                }

                if (ConfigurationConversion.TryGetValue(name, out var converter) && !string.IsNullOrEmpty(converter.propertyName))
                {
                    SetPropertyValue(
                        value,
                        converter.propertyName,
                        converter.conversionFunction);
                }

                AdditionalConfiguration(model, name, value);
            }

            CheckForMissingSettings();
        }

        protected abstract void AdditionalConfiguration(BingoServerSettingsModel model, string name, string value);

        protected void AccountForRequiredSettings(string name)
        {
            RequiredSettings.Remove(name);
        }

        protected void CheckForMissingSettings()
        {
            foreach (var setting in RequiredSettings)
            {
                LogRequiredSettingMissing(setting);
            }

            if (RequiredSettings.Count > 0)
            {
                MissingSettingsDisable();
            }
        }

        protected virtual bool IsSettingInvalid(string name, string value)
        {
            return false;
        }

        protected virtual bool SettingChangedThatRequiresNvRamClear(string name, string value, BingoServerSettingsModel model)
        {
            return false;
        }

        protected void SetPropertyValue(string value, string propertyName, Func<string, object> func)
        {
            var result = func?.Invoke(value) ?? value;
            PropertiesManager.SetProperty(propertyName, result);
        }

        protected void MissingSettingsDisable()
        {
            SystemDisable.Disable(
                BingoConstants.MissingSettingsDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoMissingRequiredSettings),
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoMissingRequiredSettingsHelp));
        }

        protected bool SettingChanged(string currentValue, string newValue)
        {
            // always pass if the currentValue hasn't been set yet (initial boot up)
            var result = !string.IsNullOrEmpty(currentValue) &&
                         !string.Equals(currentValue, newValue, StringComparison.Ordinal);
            if (result)
            {
                Logger.Debug($"current '{currentValue}' new '{newValue}'");
            }

            return result;
        }

        protected bool SettingChanged<TType>(TType currentValue, TType newValue, TType defaultValue = default)
        {
            // always pass if the currentValue hasn't been set yet (initial boot up)
            var result = !EqualityComparer<TType>.Default.Equals(currentValue, defaultValue) &&
                         !EqualityComparer<TType>.Default.Equals(currentValue, newValue);
            if (result)
            {
                Logger.Debug($"current '{currentValue}' new '{newValue}'");
            }

            return result;
        }

        protected static bool StringToBool(string input)
        {
            return input.ToLower() switch
            {
                "1" => true,
                "enable" => true,
                "enabled" => true,
                "external" => true,
                "on" => true,
                "true" => true,
                _ => false
            };
        }

        protected static bool IsBooleanValue(string input)
        {
            return StringToBool(input) || input.ToLower() switch
            {
                "off" => true,
                "0" => true,
                "false" => true,
                "disabled" => true,
                _ => false
            };
        }

        protected void LogInvalidSetting(string name, string value, ConfigurationFailureReason reason)
        {
            Logger.Warn($"Invalid config: {name}='{value}' - {reason}");
            throw new ConfigurationException(name, reason);
        }

        protected void LogUnhandledSetting(string name, string value)
        {
            Logger.Warn($"Unhandled config: {name}='{value}'");
        }

        protected void LogRequiredSettingMissing(string name)
        {
            Logger.Warn($"Missing required config: {name}");
        }
    }
}
