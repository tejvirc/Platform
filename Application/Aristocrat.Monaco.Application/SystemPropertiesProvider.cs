namespace Aristocrat.Monaco.Application
{
    using Contracts;
    using Contracts.Operations;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts.Protocol;

    /// <summary>
    ///     This class persists general system property settings and provides get and set access as a property provider
    ///     registered with the IPropertiesManager service.
    /// </summary>
    public class SystemPropertiesProvider : IPropertyProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Static;

        private const string OperatingHoursBlock = @"OperatingHours";
        private const string OperatingHoursDay = @"OperatingHours.Day";
        private const string OperatingHoursTime = @"OperatingHours.Time";
        private const string OperatingHoursEnabled = @"OperatingHours.Enabled";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly bool _blockExists;

        private readonly Dictionary<string, Tuple<object, string>> _properties;

        private readonly IPersistentStorageManager _storageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemPropertiesProvider" /> class.
        /// </summary>
        public SystemPropertiesProvider()
        {
            _storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            _blockExists = _storageManager.BlockExists(GetBlockName());

            // The Tuple is structured as value (Item1) and storageKey (Item2)
            _properties = new Dictionary<string, Tuple<object, string>>
            {
                {
                    ApplicationConstants.IsInitialConfigurationComplete,
                    Tuple.Create(
                        (object)InitFromStorage<bool>(ApplicationConstants.IsInitialConfigurationComplete),
                        ApplicationConstants.IsInitialConfigurationComplete)
                },
                {
                    ApplicationConstants.NoteAcceptorEnabled,
                    Tuple.Create((object)InitFromStorage<bool>("NoteAcceptorEnabled"), "NoteAcceptorEnabled")
                },
                {
                    ApplicationConstants.NoteAcceptorManufacturer,
                    Tuple.Create((object)InitFromStorage<string>("NoteAcceptorManufacturer"), "NoteAcceptorManufacturer")
                },
                {
                    ApplicationConstants.PrinterEnabled,
                    Tuple.Create((object)InitFromStorage<bool>("PrinterEnabled"), "PrinterEnabled")
                },
                {
                    ApplicationConstants.PrinterManufacturer,
                    Tuple.Create((object)InitFromStorage<string>("PrinterManufacturer"), "PrinterManufacturer")
                },                
                {
                    ApplicationConstants.SerialNumber,
                    Tuple.Create((object)InitFromStorage<string>("SerialNumber2"), "SerialNumber2")
                },
                {
                    ApplicationConstants.MachineId,
                    Tuple.Create((object)InitFromStorage<uint>("MachineId2"), "MachineId2")
                },
                {
                    ApplicationConstants.Area,
                    Tuple.Create((object)InitFromStorage<string>("Area"), "Area")
                },
                {
                    ApplicationConstants.Zone,
                    Tuple.Create((object)InitFromStorage<string>("Zone"), "Zone")
                },
                {
                    ApplicationConstants.Bank,
                    Tuple.Create((object)InitFromStorage<string>("Bank"), "Bank")
                },
                {
                    ApplicationConstants.Position,
                    Tuple.Create((object)InitFromStorage<string>("Position"), "Position")
                },
                {
                    ApplicationConstants.Location,
                    Tuple.Create((object)InitFromStorage<string>("Location"), "Location")
                },
                {
                    ApplicationConstants.CalculatedDeviceName,
                    Tuple.Create((object)InitFromStorage<string>("CalculatedDeviceName"), "CalculatedDeviceName")
                },
                {
                    ApplicationConstants.CurrencyId,
                    Tuple.Create((object)InitFromStorage<string>("CurrencyId"), "CurrencyId")
                },
                {
                    ApplicationConstants.CurrencyDescription,
                    Tuple.Create((object)InitFromStorage<string>("CurrencyDescription"), "CurrencyDescription")
                },
                {
                    ApplicationConstants.OperatingHours,
                    Tuple.Create((object)InitOperatingHoursFromStorage(), ApplicationConstants.OperatingHours)
                },
                {
                    ApplicationConstants.JurisdictionKey,
                    Tuple.Create((object)InitFromStorage<string>(ApplicationConstants.JurisdictionKey), ApplicationConstants.JurisdictionKey)
                },
                {
                    ApplicationConstants.ShowMode,
                    Tuple.Create((object)InitFromStorage<bool>(ApplicationConstants.ShowMode), ApplicationConstants.ShowMode)
                },
                {
                    ApplicationConstants.GameRules,
                    Tuple.Create((object)InitFromStorage(ApplicationConstants.GameRules, true), ApplicationConstants.GameRules)
                },
                {
                    ApplicationConstants.DemonstrationMode,
                    Tuple.Create(
                        (object)InitFromStorage(ApplicationConstants.DemonstrationMode, false),
                        ApplicationConstants.DemonstrationMode)
                },
                {
                    ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled,
                    Tuple.Create(
                        (object)InitFromStorage<bool>(
                            ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled),
                        ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled)
                },
                {
                    ApplicationConstants.CabinetBrightnessControlEnabled,
                    Tuple.Create((object)false, string.Empty)
                },
                {
                    ApplicationConstants.CabinetBrightnessControlDefault,
                    Tuple.Create((object)50, string.Empty)
                },
                {
                    ApplicationConstants.CabinetBrightnessControlMin,
                    Tuple.Create((object)10, string.Empty)
                },
                {
                    ApplicationConstants.CabinetBrightnessControlMax,
                    Tuple.Create((object)100, string.Empty)
                },
                {
                    ApplicationConstants.EdgeLightingBrightnessControlEnabled,
                    Tuple.Create((object)false, string.Empty)
                },
                {
                    ApplicationConstants.EdgeLightingBrightnessControlDefault,
                    Tuple.Create((object)50, string.Empty)
                },
                {
                    ApplicationConstants.EdgeLightingBrightnessControlMin,
                    Tuple.Create((object)10, string.Empty)
                },
                {
                    ApplicationConstants.EdgeLightingBrightnessControlMax,
                    Tuple.Create((object)100, string.Empty)
                },
                {
                    ApplicationConstants.BottomStripEnabled,
                    Tuple.Create((object)false, string.Empty)
                },
                {
                    ApplicationConstants.HardMeterMapSelectionValue,
                    Tuple.Create((object)InitFromStorage<string>(ApplicationConstants.HardMeterMapSelectionValue), ApplicationConstants.HardMeterMapSelectionValue)
                },
                {
                    ApplicationConstants.CommunicationsOnline,
                    Tuple.Create((object)false, string.Empty)
                },
                {
                    ApplicationConstants.CentralAllowed,
                    Tuple.Create((object)false, string.Empty)
                },
                {
                    ApplicationConstants.IdReaderEnabled,
                    Tuple.Create((object)InitFromStorage<bool>("IdReaderEnabled"), "IdReaderEnabled")
                },
                {
                    ApplicationConstants.IdReaderManufacturer,
                    Tuple.Create((object)InitFromStorage<string>("IdReaderManufacturer"), "IdReaderManufacturer")
                },
                {
                    ApplicationConstants.ReelControllerEnabled,
                    Tuple.Create((object)InitFromStorage<bool>("ReelControllerEnabled"), "ReelControllerEnabled")
                },
                {
                    ApplicationConstants.ReelControllerManufacturer,
                    Tuple.Create((object)InitFromStorage<string>("ReelControllerManufacturer"), "ReelControllerManufacturer")
                },
                {
                    ApplicationConstants.MachineSettingsImported,
                    Tuple.Create((object)InitFromStorage<int>(ApplicationConstants.MachineSettingsImported), ApplicationConstants.MachineSettingsImported)
                },
                {
                    ApplicationConstants.MachineSettingsReimport,
                    Tuple.Create((object)InitFromStorage<bool>(ApplicationConstants.MachineSettingsReimport), ApplicationConstants.MachineSettingsReimport)
                },
                {
                    ApplicationConstants.MachineSettingsReimported,
                    Tuple.Create((object)false, string.Empty)
                },
                {
                    ApplicationConstants.LegalCopyrightAcceptedKey,
                    Tuple.Create(
                        (object)InitFromStorage<bool>(ApplicationConstants.LegalCopyrightAcceptedKey),
                        ApplicationConstants.LegalCopyrightAcceptedKey)
                },
                {
                    ApplicationConstants.PreviousGameEndTotalBanknotesInKey,
                    Tuple.Create(
                        (object)InitFromStorage<long>(ApplicationConstants.PreviousGameEndTotalBanknotesInKey),
                        ApplicationConstants.PreviousGameEndTotalBanknotesInKey)
                },
                {
                    ApplicationConstants.PreviousGameEndTotalCoinInKey,
                    Tuple.Create(
                        (object)InitFromStorage<long>(ApplicationConstants.PreviousGameEndTotalCoinInKey),
                        ApplicationConstants.PreviousGameEndTotalCoinInKey)
                },
                {
                    ApplicationConstants.ExcessiveMeterIncrementLockedKey,
                    Tuple.Create(
                        (object)InitFromStorage<bool>(ApplicationConstants.ExcessiveMeterIncrementLockedKey),
                        ApplicationConstants.ExcessiveMeterIncrementLockedKey)
                },
                {
                    ApplicationConstants.EKeyVerified,
                    Tuple.Create((object)false, string.Empty)
                },
                {
                    ApplicationConstants.EKeyDrive,
                    Tuple.Create((object)string.Empty, string.Empty)
                },
                {
                    ApplicationConstants.HostAddresses,
                    Tuple.Create((object)InitFromStorage<string>(ApplicationConstants.HostAddresses), ApplicationConstants.HostAddresses)
                }
            };

            _blockExists = true;
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

            var errorMessage = "Unknown game property: " + propertyName;
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
                Validate(propertyName, propertyValue);

                // NOTE:  Not all properties are persisted
                if (!string.IsNullOrEmpty(value.Item2))
                {
                    Logger.Debug(
                        $"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");

                    switch (propertyName)
                    {
                        case ApplicationConstants.OperatingHours:
                            var operatingHours = propertyValue as IEnumerable;

                            propertyValue = UpdateOperatingHours(operatingHours?.Cast<OperatingHours>());
                            break;
                        default:
                            var accessor = GetAccessor();
                            accessor[value.Item2] = propertyValue;
                            break;
                    }
                }

                _properties[propertyName] = Tuple.Create(propertyValue, value.Item2);
            }
        }

        private void Validate(string propertyName, object propertyValue)
        {
            switch (propertyName)
            {
                case ApplicationConstants.MachineId:
                    var value = propertyValue.ToString();
                    var protocols = ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>()
                        .MultiProtocolConfiguration
                        .Select(x => x.Protocol);
                    var maxLength = protocols.Contains(CommsProtocol.SAS)
                        ? ApplicationConstants.MaxMachineIdLengthSAS
                        : ApplicationConstants.MaxMachineIdLengthG2S;

                    if (value.Length > maxLength)
                    {
                        const string message = "MachineId exceeds the length limit of 8.";
                        Logger.Fatal(message);
                        throw new ArgumentOutOfRangeException(propertyName, message);
                    }

                    break;
                case ApplicationConstants.OperatingHours:
                    if (!(propertyValue is IEnumerable operatingHours) ||
                        operatingHours.Cast<OperatingHours>().Count() > ApplicationConstants.MaxOperatingHours)
                    {
                        const string message = "Operating hours is the wrong type or contains too many elements.";
                        Logger.Fatal(message);
                        throw new ArgumentOutOfRangeException(propertyName, message);
                    }

                    break;
            }
        }

        private static IEnumerable<OperatingHours> GetDefaultOperatingHours(bool enabled)
        {
            return Enumerable.Range(0, 7).Select(
                day => new OperatingHours
                {
                    Day = (DayOfWeek)day,
                    Time = 0,
                    Enabled = enabled
                });
        }

        private IPersistentStorageAccessor GetAccessor(string name = null, int blockSize = 1)
        {
            var blockName = GetBlockName(name);

            return _storageManager.BlockExists(blockName)
                ? _storageManager.GetBlock(blockName)
                : _storageManager.CreateBlock(Level, blockName, blockSize);
        }

        private string GetBlockName(string name = null)
        {
            var baseName = GetType().ToString();
            return string.IsNullOrEmpty(name) ? baseName : $"{baseName}.{name}";
        }

        private T InitFromStorage<T>(string propertyName, T defaultValue = default(T))
        {
            var accessor = GetAccessor();
            if (!_blockExists)
            {
                accessor[propertyName] = defaultValue;
            }

            return (T)accessor[propertyName];
        }

        private IEnumerable<OperatingHours> InitOperatingHoursFromStorage()
        {
            var blockName = GetBlockName(OperatingHoursBlock);

            // NOTE:  This is only here to make sure we have an uninitialized collection.
            //        Need to be able to specify the default value when we define the blocks vs. this.
            var exists = _storageManager.BlockExists(blockName);
            if (!exists)
            {
                _storageManager.CreateBlock(Level, blockName, ApplicationConstants.MaxOperatingHours);

                UpdateOperatingHours(GetDefaultOperatingHours(true));
            }

            var operatingHours = new List<OperatingHours>();
            var accessor = GetAccessor(OperatingHoursBlock, ApplicationConstants.MaxOperatingHours);

            var days = accessor.GetAll();

            for (var index = 0; index < ApplicationConstants.MaxOperatingHours; index++)
            {
                if (!days.TryGetValue(index, out var values))
                {
                    continue;
                }

                var day = values[OperatingHoursDay];

                if (Enum.IsDefined(typeof(DayOfWeek), day))
                {
                    operatingHours.Add(
                        new OperatingHours
                        {
                            Day = (DayOfWeek)day,
                            Time = (int)values[OperatingHoursTime],
                            Enabled = (bool)values[OperatingHoursEnabled]
                        });
                }
            }

            return operatingHours;
        }

        private IEnumerable<OperatingHours> UpdateOperatingHours(IEnumerable<OperatingHours> operatingHours)
        {
            if (operatingHours == null)
            {
                return null;
            }

            var operatingHoursAccessor = GetAccessor(OperatingHoursBlock, ApplicationConstants.MaxOperatingHours);

            var list = operatingHours.ToList();

            if (list.Count == 0)
            {
                // Disable all days if the list has been cleared
                list.AddRange(GetDefaultOperatingHours(false));
            }

            for (var index = 0; index < ApplicationConstants.MaxOperatingHours; index++)
            {
                var item = index >= list.Count ? null : list.ElementAt(index);

                operatingHoursAccessor[index, OperatingHoursDay] = item?.Day ?? (DayOfWeek?)-1;
                operatingHoursAccessor[index, OperatingHoursTime] = item?.Time ?? 0;
                operatingHoursAccessor[index, OperatingHoursEnabled] = item?.Enabled ?? false;
            }

            return list;
        }
    }
}
