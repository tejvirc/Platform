namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.TowerLight;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     This class loads operator menu configurations based on jurisdiction into ConfigWizard properties and provides
    ///     get and set access as a property provider registered with the IPropertiesManager service.
    /// </summary>
    public class ConfigWizardConfigurationPropertiesProvider : IPropertyProvider
    {
        private const string ConfigWizardConfigurationExtensionPath = "/ConfigWizard/Configuration";
        private const long DefaultHardMeterTickValue = 100;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly bool _blockExists;
        private readonly IPersistentStorageAccessor _persistentStorageAccessor;

        private readonly Dictionary<string, Tuple<object, string>> _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigWizardConfigurationPropertiesProvider" /> class.
        /// </summary>
        public ConfigWizardConfigurationPropertiesProvider()
        {
            var storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            var blockName = GetType().ToString();
            _blockExists = storageManager.BlockExists(blockName);

            _persistentStorageAccessor = _blockExists
                ? storageManager.GetBlock(blockName)
                : storageManager.CreateBlock(PersistenceLevel.Static, blockName, 1);

            var configWizardConfiguration = ConfigurationUtilities.GetConfiguration(
                ConfigWizardConfigurationExtensionPath,
                () => new ConfigWizardConfiguration());

            var configWizardDoorOpticsEnabled = configWizardConfiguration.DoorOptics?.Enabled ?? false;
            var configWizardBellyPanelDoorEnabled = configWizardConfiguration.BellyPanelDoor?.Enabled ?? true;
            var configWizardBellEnabled = configWizardConfiguration.Bell?.Enabled ?? false;
            var hardMeterTickValue = DefaultHardMeterTickValue;
            var enterOutOfServiceWithCreditsEnabled =
                configWizardConfiguration.MachineSetupConfig?.EnterOutOfServiceWithCredits?.Enabled ?? true;
            var hardMetersEnabled = configWizardConfiguration.HardMetersConfig?.Enable ?? false;

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var machineSettingsImported = propertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None);
            if (!_blockExists && machineSettingsImported != ImportMachineSettings.None)
            {
                configWizardDoorOpticsEnabled = propertiesManager.GetValue(ApplicationConstants.ConfigWizardDoorOpticsEnabled, false);
                configWizardBellEnabled = propertiesManager.GetValue(HardwareConstants.BellEnabledKey, false);
                hardMeterTickValue = propertiesManager.GetValue(ApplicationConstants.HardMeterTickValue, DefaultHardMeterTickValue);
                enterOutOfServiceWithCreditsEnabled = propertiesManager.GetValue(ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled, true);
                hardMetersEnabled = propertiesManager.GetValue(HardwareConstants.HardMetersEnabledKey, false);
            }

            // The Tuple is structured as value (Item1) and Key (Item2)
            _properties = new Dictionary<string, Tuple<object, string>>
            {
                {
                    ApplicationConstants.ConfigWizardMachineSetupConfigVisibility,
                    Tuple.Create(
                        (object)(configWizardConfiguration.MachineSetupConfig?.Visibility != null &&
                                 configWizardConfiguration.MachineSetupConfig.Visibility.Equals(ApplicationConstants.Visible)),
                        ApplicationConstants.ConfigWizardMachineSetupConfigVisibility)
                },
                {
                    ApplicationConstants.ConfigWizardIdentityPageVisibility,
                    Tuple.Create(
                        (object)configWizardConfiguration.IdentityPage?.Visibility?.Equals(ApplicationConstants.Visible) ?? false,
                        ApplicationConstants.ConfigWizardIdentityPageVisibility)
                },
                {
                    ApplicationConstants.ConfigWizardUseSelectionEnabled,
                    Tuple.Create(
                        (object)configWizardConfiguration.IOConfigPage?.UseSelection?.Enabled ?? false,
                        ApplicationConstants.ConfigWizardUseSelectionEnabled)
                },
                {
                    ApplicationConstants.ConfigWizardPrintIdentityEnabled,
                    Tuple.Create(
                        (object)configWizardConfiguration.IdentityPage?.PrintIdentity?.Visibility?.Equals(ApplicationConstants.Visible) ?? false,
                        ApplicationConstants.ConfigWizardPrintIdentityEnabled)
                },
                {
                    ApplicationConstants.ConfigWizardCompletionPageShowGameSetupMessage,
                    Tuple.Create(
                        (object)(configWizardConfiguration.CompletionPage?.ShowGameSetupMessage ?? false),
                        ApplicationConstants.ConfigWizardCompletionPageShowGameSetupMessage)
                },
                {
                    ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEditable,
                    Tuple.Create(
                        (object)configWizardConfiguration.MachineSetupConfig?.EnterOutOfServiceWithCredits?.Editable ?? true,
                        ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEditable)
                },
                {
                    ApplicationConstants.TowerLightTierTypeSelection,
                    Tuple.Create((object)InitFromStorage(ApplicationConstants.TowerLightTierTypeSelection, TowerLightTierTypes.Undefined.ToString()),
                        ApplicationConstants.TowerLightTierTypeSelection)
                },
                {
                    ApplicationConstants.ConfigWizardHardMetersConfigConfigurable,
                    Tuple.Create(
                        (object)(configWizardConfiguration.HardMetersConfig?.Configurable ?? true),
                         ApplicationConstants.ConfigWizardHardMetersConfigConfigurable)
                },
                {
                    ApplicationConstants.ConfigWizardHardMetersConfigCanReconfigure,
                    Tuple.Create(
                        (object)(configWizardConfiguration.HardMetersConfig?.CanReconfigure ?? false),
                        ApplicationConstants.ConfigWizardHardMetersConfigCanReconfigure)
                },
                {
                    ApplicationConstants.ConfigWizardHardMetersConfigVisible,
                    Tuple.Create(
                        (object)(configWizardConfiguration.HardMetersConfig?.Visible ?? true),
                        ApplicationConstants.ConfigWizardHardMetersConfigVisible)
                },
                {
                    ApplicationConstants.ConfigWizardLimitsPageEnabled,
                    Tuple.Create(
                        (object)(configWizardConfiguration.LimitsPage?.Enabled ?? true),
                         ApplicationConstants.ConfigWizardLimitsPageEnabled)
                },
                {
                    ApplicationConstants.ConfigWizardCreditLimitCheckboxEditable,
                    Tuple.Create(
                        (object)(configWizardConfiguration.LimitsPage?.CreditLimit?.CheckboxEditable ?? true),
                        ApplicationConstants.ConfigWizardCreditLimitCheckboxEditable)
                },
                {
                    ApplicationConstants.ConfigWizardHandpayLimitVisible,
                    Tuple.Create(
                        (object)(configWizardConfiguration.LimitsPage?.HandpayLimit?.Visible ?? true),
                        ApplicationConstants.ConfigWizardHandpayLimitVisible)
                },
                {
                    ApplicationConstants.ConfigWizardHandpayLimitCheckboxEditable,
                    Tuple.Create(
                        (object)(configWizardConfiguration.LimitsPage?.HandpayLimit?.CheckboxEditable ?? true),
                        ApplicationConstants.ConfigWizardHandpayLimitCheckboxEditable)
                },
                {
                    ApplicationConstants.ConfigWizardDoorOpticsVisible,
                    Tuple.Create(
                        (object)(configWizardConfiguration.DoorOptics?.Visible ?? false),
                        ApplicationConstants.ConfigWizardDoorOpticsVisible)
                },
                {
                    ApplicationConstants.ConfigWizardDoorOpticsConfigurable,
                    Tuple.Create(
                        (object)(configWizardConfiguration.DoorOptics?.Configurable ?? false),
                        ApplicationConstants.ConfigWizardDoorOpticsConfigurable)
                },
                {
                    ApplicationConstants.ConfigWizardDoorOpticsCanReconfigure,
                    Tuple.Create(
                        (object)(configWizardConfiguration.DoorOptics?.CanReconfigure ?? false),
                        ApplicationConstants.ConfigWizardDoorOpticsCanReconfigure)
                },
                {
                    ApplicationConstants.ConfigWizardDoorOpticsEnabled,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.ConfigWizardDoorOpticsEnabled,
                            configWizardDoorOpticsEnabled),
                        ApplicationConstants.ConfigWizardDoorOpticsEnabled)
                },
                {
                    ApplicationConstants.ConfigWizardBellyPanelDoorVisible,
                    Tuple.Create(
                        (object)(configWizardConfiguration.BellyPanelDoor?.Visible ?? false),
                        ApplicationConstants.ConfigWizardBellyPanelDoorVisible)
                },
                {
                    ApplicationConstants.ConfigWizardBellyPanelDoorConfigurable,
                    Tuple.Create(
                        (object)(configWizardConfiguration.BellyPanelDoor?.Configurable ?? false),
                        ApplicationConstants.ConfigWizardBellyPanelDoorConfigurable)
                },
                {
                    ApplicationConstants.ConfigWizardBellyPanelDoorCanReconfigure,
                    Tuple.Create(
                        (object)(configWizardConfiguration.BellyPanelDoor?.CanReconfigure ?? false),
                        ApplicationConstants.ConfigWizardBellyPanelDoorCanReconfigure)
                },
                {
                    ApplicationConstants.ConfigWizardBellyPanelDoorEnabled,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.ConfigWizardBellyPanelDoorEnabled,
                            configWizardBellyPanelDoorEnabled),
                        ApplicationConstants.ConfigWizardBellyPanelDoorEnabled)
                },
                {
                    ApplicationConstants.ConfigWizardBellVisible,
                    Tuple.Create(
                        (object)(configWizardConfiguration.Bell?.Visible ?? false),
                        ApplicationConstants.ConfigWizardBellVisible)
                },
                {
                    ApplicationConstants.ConfigWizardBellConfigurable,
                    Tuple.Create(
                        (object)(configWizardConfiguration.Bell?.Configurable ?? false),
                        ApplicationConstants.ConfigWizardBellConfigurable)
                },
                {
                    ApplicationConstants.ConfigWizardBellCanReconfigure,
                    Tuple.Create(
                        (object)(configWizardConfiguration.Bell?.CanReconfigure ?? false),
                        ApplicationConstants.ConfigWizardBellCanReconfigure)
                },
                {
                    ApplicationConstants.HardMeterTickValueConfigurable,
                    Tuple.Create(
                        (object)(configWizardConfiguration.HardMetersConfig?.TickValueConfigurable ?? false),
                        ApplicationConstants.HardMeterTickValueConfigurable)
                },
                {
                    ApplicationConstants.HardMeterTickValue,
                    Tuple.Create(
                        (object)InitFromStorage(
                            ApplicationConstants.HardMeterTickValue,
                            hardMeterTickValue),
                        ApplicationConstants.HardMeterTickValue)
                },
                {
                    ApplicationConstants.ConfigWizardHardwarePageRequirePrinter,
                    Tuple.Create(
                        (object)(configWizardConfiguration.HardwarePage?.RequirePrinter ?? false),
                        ApplicationConstants.ConfigWizardHardwarePageRequirePrinter)
                }
            };

            if (configWizardConfiguration.IdentityPage != null)
            {
                if (configWizardConfiguration.IdentityPage.SerialNumber != null)
                {
                    _properties[ApplicationConstants.ConfigWizardIdentityPageSerialNumberOverride] =
                        Tuple.Create(
                            (object)configWizardConfiguration.IdentityPage.SerialNumber,
                            ApplicationConstants.ConfigWizardIdentityPageSerialNumberOverride);
                }

                if (configWizardConfiguration.IdentityPage.AssetNumber != null)
                {
                    _properties[ApplicationConstants.ConfigWizardIdentityPageAssetNumberOverride] =
                        Tuple.Create(
                            (object)configWizardConfiguration.IdentityPage.AssetNumber,
                            ApplicationConstants.ConfigWizardIdentityPageAssetNumberOverride);
                }

                if (configWizardConfiguration.IdentityPage.Area != null)
                {
                    _properties[ApplicationConstants.ConfigWizardIdentityPageAreaOverride] =
                        Tuple.Create(
                            (object)configWizardConfiguration.IdentityPage.Area,
                            ApplicationConstants.ConfigWizardIdentityPageAreaOverride);
                }

                if (configWizardConfiguration.IdentityPage.Zone != null)
                {
                    _properties[ApplicationConstants.ConfigWizardIdentityPageZoneOverride] =
                        Tuple.Create(
                            (object)configWizardConfiguration.IdentityPage.Zone,
                            ApplicationConstants.ConfigWizardIdentityPageZoneOverride);
                }

                if (configWizardConfiguration.IdentityPage.Bank != null)
                {
                    _properties[ApplicationConstants.ConfigWizardIdentityPageBankOverride] =
                        Tuple.Create(
                            (object)configWizardConfiguration.IdentityPage.Bank,
                            ApplicationConstants.ConfigWizardIdentityPageBankOverride);
                }

                if (configWizardConfiguration.IdentityPage.Position != null)
                {
                    _properties[ApplicationConstants.ConfigWizardIdentityPagePositionOverride] =
                        Tuple.Create(
                            (object)configWizardConfiguration.IdentityPage.Position,
                            ApplicationConstants.ConfigWizardIdentityPagePositionOverride);
                }

                if (configWizardConfiguration.IdentityPage.Location != null)
                {
                    _properties[ApplicationConstants.ConfigWizardIdentityPageLocationOverride] =
                        Tuple.Create(
                            (object)configWizardConfiguration.IdentityPage.Location,
                            ApplicationConstants.ConfigWizardIdentityPageLocationOverride);
                }

                if (configWizardConfiguration.IdentityPage.DeviceName != null)
                {
                    _properties[ApplicationConstants.ConfigWizardIdentityPageDeviceNameOverride] =
                        Tuple.Create(
                            (object)configWizardConfiguration.IdentityPage.DeviceName,
                            ApplicationConstants.ConfigWizardIdentityPageDeviceNameOverride);
                }
            }

            if (configWizardConfiguration.ProtocolConfiguration?.ProtocolsAllowed != null)
            {
                _properties.Add(ApplicationConstants.ProtocolsAllowedKey,
                    Tuple.Create(
                        (object)configWizardConfiguration.ProtocolConfiguration.ProtocolsAllowed.Select(x => x.Name).ToArray(),
                        ApplicationConstants.ProtocolsAllowedKey));

                _properties.Add(ApplicationConstants.MandatoryProtocol,
                    Tuple.Create(
                    (object)configWizardConfiguration.ProtocolConfiguration.ProtocolsAllowed.FirstOrDefault(x => x.IsMandatory)?.Name,
                    ApplicationConstants.MandatoryProtocol));
            }

            if (!_blockExists)
            {
                if (machineSettingsImported != ImportMachineSettings.None)
                {
                    machineSettingsImported |= ImportMachineSettings.ConfigWizardConfigurationPropertiesLoaded;
                    propertiesManager.SetProperty(ApplicationConstants.MachineSettingsImported, machineSettingsImported);
                }

                // another property provider persists these settings
                propertiesManager.SetProperty(ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled, enterOutOfServiceWithCreditsEnabled);
                propertiesManager.SetProperty(HardwareConstants.HardMetersEnabledKey, hardMetersEnabled);
                propertiesManager.SetProperty(HardwareConstants.BellEnabledKey, configWizardBellEnabled);
            }

            propertiesManager.AddPropertyProvider(this);
        }

        private T InitFromStorage<T>(string propertyName, T defaultValue = default)
        {
            if (!_blockExists)
            {
                _persistentStorageAccessor[propertyName] = defaultValue;
            }

            return (T)_persistentStorageAccessor[propertyName];
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection
            =>
                new List<KeyValuePair<string, object>>(
                    _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Item1)));

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value.Item1;
            }

            var errorMessage = "Unknown config wizard property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out var value))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            if (value.Item1 != propertyValue)
            {
                if (!string.IsNullOrEmpty(value.Item2))
                {
                    Logger.Debug(
                        $"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");
                }

                // NOTE:  Not all properties are persisted
                switch (propertyName)
                {
                    case ApplicationConstants.ConfigWizardDoorOpticsEnabled:
                    case ApplicationConstants.ConfigWizardBellyPanelDoorEnabled:
                    case ApplicationConstants.HardMeterTickValue:
                    case ApplicationConstants.TowerLightTierTypeSelection:
                        _persistentStorageAccessor[propertyName] = propertyValue;
                        break;
                }

                _properties[propertyName] = Tuple.Create(propertyValue, value.Item2);
            }
        }
    }
}
