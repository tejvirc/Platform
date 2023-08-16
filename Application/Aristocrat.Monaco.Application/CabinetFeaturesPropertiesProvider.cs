namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using Contracts;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Hardware.Contracts.Audio;
    using Aristocrat.Cabinet.Contracts;

    /// <summary>
    ///     Definition of the ApplicationConfigurationPropertiesProvider class
    /// </summary>
    public class CabinetFeaturesPropertiesProvider : IPropertyProvider
    {
        private const string CabinetFeaturesXml = @".\CabinetFeatures.xml";
        private const PersistenceLevel Level = PersistenceLevel.Static;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly Dictionary<string, Tuple<object, string, bool>> _properties;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IPersistentStorageManager _storageManager;
        private readonly bool _blockExists;
        private readonly ImportMachineSettings _machineSettingsImported;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CabinetFeaturesPropertiesProvider" /> class.
        /// </summary>
        public CabinetFeaturesPropertiesProvider() 
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>()
                )
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationConfigurationPropertiesProvider"/> class.
        /// </summary>
        /// <param name="propertiesManager"></param>
        /// <param name="storageManager"></param>
        public CabinetFeaturesPropertiesProvider(
            IPropertiesManager propertiesManager,
            IPersistentStorageManager storageManager
            )
        {
            _propertiesManager = propertiesManager;
            _storageManager = storageManager;

            _properties = new Dictionary<string, Tuple<object, string, bool>>();
            _blockExists = _storageManager.BlockExists(GetBlockName());
            _machineSettingsImported = _propertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None);

            SetCabinetConfiguration();

            if (_machineSettingsImported != ImportMachineSettings.None)
            { 
                _machineSettingsImported |= ImportMachineSettings.CabinetFeaturesPropertiesLoaded;
                propertiesManager.SetProperty(ApplicationConstants.MachineSettingsImported, _machineSettingsImported);
            }

            _propertiesManager.AddPropertyProvider(this);
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

            var errorMessage = "Unknown application property: " + propertyName;
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

                //Item3 indicates persistence
                if (value.Item3)
                {
                    var accessor = GetAccessor();
                    accessor[value.Item2] = propertyValue;
                }

                _properties[propertyName] = Tuple.Create(propertyValue, value.Item2, value.Item3);
            }
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

        private void InitializeSoundChannelProperties(FeaturesSoundChannel[] featuresSoundChannel, CabinetType cabinetType)
        {
            var soundChannel =
                featuresSoundChannel.FirstOrDefault(x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex)) ??
                new FeaturesSoundChannel();

            var enabledSpeakersMask = SpeakerMix.None;

            foreach (var channel in soundChannel.Channel)
            {
                Enum.TryParse(channel, out SpeakerMix parsedChannel);
                if (parsedChannel == SpeakerMix.All)
                {
                    enabledSpeakersMask = SpeakerMix.All;
                    break;
                }
                enabledSpeakersMask |= parsedChannel;
            }

            _properties.Add(
               ApplicationConstants.EnabledSpeakersMask,
               Tuple.Create(
               (object)enabledSpeakersMask,
               ApplicationConstants.EnabledSpeakersMask,
                false));
        }

        private void InitializeMalfunctionMessageProperties(
            IEnumerable<FeaturesMalfunctionMessage> cabinetFeaturesMalfunctionMessage,
            CabinetType cabinetType)
        {
            var malfunctionMessage = cabinetFeaturesMalfunctionMessage.FirstOrDefault(
                                         x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex))
                                     ?? new FeaturesMalfunctionMessage { Enabled = false };

            _properties.Add(
                ApplicationConstants.EnabledMalfunctionMessage,
                Tuple.Create(
                    (object)malfunctionMessage.Enabled,
                    ApplicationConstants.EnabledMalfunctionMessage,
                    false)
            );
        }


        private void InitializeScreenBrightnessProperties(FeaturesScreenBrightnessControl[] featuresScreenBrightnessControl, CabinetType cabinetType)
        {
            var feature = featuresScreenBrightnessControl.FirstOrDefault(x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex)) ?? new FeaturesScreenBrightnessControl
            {
                Default = ApplicationConstants.DefaultBrightness,
                Enabled = false,
                Min = 0,
                Max = ApplicationConstants.DefaultBrightness
            };

            if (!_blockExists && _machineSettingsImported != ImportMachineSettings.None)
            {
                feature.Default = _propertiesManager.GetValue(ApplicationConstants.ScreenBrightness, ApplicationConstants.DefaultBrightness);
            }

            _properties.Add(
                ApplicationConstants.ScreenBrightness,
                Tuple.Create(
                    (object)InitFromStorage(
                        ApplicationConstants.ScreenBrightness,
                        feature.Default),
                    ApplicationConstants.ScreenBrightness,
                    true)
            );

            _properties.Add(
                ApplicationConstants.CabinetBrightnessControlEnabled,
                Tuple.Create((object)feature.Enabled,
                    ApplicationConstants.CabinetBrightnessControlEnabled,
                    true)
            );

            if (feature.Enabled && !_blockExists)
            {
                Task.Run(() => ChangeBrightness(feature.Default));
            }

            _properties.Add(
                ApplicationConstants.CabinetBrightnessControlDefault,
                Tuple.Create((object)feature.Default,
                    ApplicationConstants.CabinetBrightnessControlDefault,
                    true)
            );
            _properties.Add(
                ApplicationConstants.CabinetBrightnessControlMin,
                Tuple.Create((object)feature.Min,
                    ApplicationConstants.CabinetBrightnessControlMin,
                    true)
            );
            _properties.Add(
                ApplicationConstants.CabinetBrightnessControlMax,
                Tuple.Create((object)feature.Max,
                    ApplicationConstants.CabinetBrightnessControlMax,
                    true)
            );
        }

        private void InitializeEdgeLightBrightnessProperties(FeaturesEdgeLightBrightnessControl[] featuresEdgeLightBrightnessControl, CabinetType cabinetType)
        {
            var edgeLightFeature = featuresEdgeLightBrightnessControl.FirstOrDefault(x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex))
              ?? new FeaturesEdgeLightBrightnessControl
              {
                  Enabled = false,
                  Default = ApplicationConstants.DefaultEdgeLightingMaximumBrightness,
                  Min = 0,
                  Max = ApplicationConstants.DefaultEdgeLightingMaximumBrightness
              };

            var maximumAllowedEdgeLightingBrightness = edgeLightFeature.Default;
            if (!_blockExists && _machineSettingsImported != ImportMachineSettings.None)
            {
                maximumAllowedEdgeLightingBrightness = _propertiesManager.GetValue(ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey, ApplicationConstants.DefaultEdgeLightingMaximumBrightness);
            }

            _properties.Add(
                ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey,
                Tuple.Create((object)InitFromStorage(
                    ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey,
                    maximumAllowedEdgeLightingBrightness),
                    ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey,
                    true)
            );

            _properties.Add(
                ApplicationConstants.EdgeLightingBrightnessControlEnabled,
                Tuple.Create((object)edgeLightFeature.Enabled,
                    ApplicationConstants.EdgeLightingBrightnessControlEnabled,
                    true)
            );

            _properties.Add(
                ApplicationConstants.EdgeLightingBrightnessControlDefault,
                Tuple.Create((object)edgeLightFeature.Default,
                    ApplicationConstants.EdgeLightingBrightnessControlDefault,
                    true)
            );

            _properties.Add(
                ApplicationConstants.EdgeLightingBrightnessControlMin,
                Tuple.Create((object)edgeLightFeature.Min,
                    ApplicationConstants.EdgeLightingBrightnessControlMin,
                    true)
            );

            _properties.Add(
                ApplicationConstants.EdgeLightingBrightnessControlMax,
                Tuple.Create((object)edgeLightFeature.Max,
                    ApplicationConstants.EdgeLightingBrightnessControlMax,
                    true)
            );
        }

        private void InitializeBottomStripProperties(FeaturesBottomStrip[] featuresBottomStrip, CabinetType cabinetType)
        {
            var bottomStripFeature = featuresBottomStrip.FirstOrDefault(x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex))
                                            ?? new FeaturesBottomStrip
                                            {
                                                Enabled = false
                                            };

            _properties.Add(
                ApplicationConstants.BottomStripEnabled,
                Tuple.Create((object)bottomStripFeature.Enabled,
                    ApplicationConstants.BottomStripEnabled,
                    true)
            );
        }

        private void InitializeEdgeLightAsTowerLightProperties(FeaturesEdgeLightAsTowerLight[] featuresEdgeLightAsTowerLight, CabinetType cabinetType)
        {
            var edgeLightAsTowerLight =
                featuresEdgeLightAsTowerLight.FirstOrDefault(
                    x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex))
                ?? new FeaturesEdgeLightAsTowerLight { Enabled = false };

            _properties.Add(
                ApplicationConstants.EdgeLightAsTowerLightEnabled,
                Tuple.Create(
                    (object)edgeLightAsTowerLight.Enabled,
                    ApplicationConstants.EdgeLightAsTowerLightEnabled,
                    false)
            );

        }

        private void InitializeBarkeeperProperties(FeaturesBarkeeper[] featuresBarkeeper, CabinetType cabinetType)
        {
            var barkeeper =
                featuresBarkeeper.FirstOrDefault(
                    x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex)) ??
                new FeaturesBarkeeper { Enabled = false };
            _properties.Add(
                ApplicationConstants.BarkeeperEnabled,
                Tuple.Create(
                    (object)barkeeper.Enabled,
                    ApplicationConstants.BarkeeperEnabled,
                    false));
        }

        private void InitializeUniversalInterfaceBoxProperties(FeaturesUniversalInterfaceBox[] featuresUniversalInterfaceBox, CabinetType cabinetType)
        {
            var universalInterfaceBox =
                featuresUniversalInterfaceBox.FirstOrDefault(
                    x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex)) ??
                new FeaturesUniversalInterfaceBox { Enabled = false };
            _properties.Add(
                ApplicationConstants.UniversalInterfaceBoxEnabled,
                Tuple.Create(
                    (object)universalInterfaceBox.Enabled,
                    ApplicationConstants.UniversalInterfaceBoxEnabled,
                    false));
        }

        private void InitializeHarkeyReelControllerProperties(FeaturesHarkeyReelController[] featuresHarkeyReelController, CabinetType cabinetType)
        {
            var harkeyReelController =
                featuresHarkeyReelController.FirstOrDefault(
                    x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex)) ??
                new FeaturesHarkeyReelController { Enabled = false };
            _properties.Add(
                ApplicationConstants.HarkeyReelControllerEnabled,
                Tuple.Create(
                    (object)harkeyReelController.Enabled,
                    ApplicationConstants.HarkeyReelControllerEnabled,
                    false));
        }

        private void InitializeDisplayElementsControllerProperties(FeaturesDisplayElementsControl[] featuresControllers, CabinetType cabinetType)
        {
            var controller = featuresControllers.FirstOrDefault(x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex))
                             ?? new FeaturesDisplayElementsControl { Enabled = false };

            _properties.Add(
                ApplicationConstants.CabinetControlsDisplayElements,
                Tuple.Create(
                    (object)controller.Enabled,
                    ApplicationConstants.CabinetControlsDisplayElements,
                    false));
        }

        private void InitializeBeagleBoneProperties(FeaturesBeagleBone[] featuresBeagleBone, CabinetType cabinetType)
        {
            var beagleBone =
                featuresBeagleBone.FirstOrDefault(
                    x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex)) ??
                new FeaturesBeagleBone { Enabled = false };
            _properties.Add(
                ApplicationConstants.BeagleBoneEnabled,
                Tuple.Create(
                    (object)beagleBone.Enabled,
                    ApplicationConstants.BeagleBoneEnabled,
                    false));
        }

        private void InitializeDisplayEdgeLightingPageProperties(FeaturesDisplayLightingPage[] featuresDisplayLightingPage, CabinetType cabinetType)
        {
            var displayLightingPage =
                featuresDisplayLightingPage.FirstOrDefault(
                    x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex)) ??
                new FeaturesDisplayLightingPage { Enabled = false };
            _properties.Add(
                ApplicationConstants.DisplayLightingPage,
                Tuple.Create(
                    (object)displayLightingPage.Enabled,
                    ApplicationConstants.DisplayLightingPage,
                    false));
		}
		
        private void InitializeKeyboardProviderProperties(FeaturesKeyboardProvider[] features, CabinetType cabinetType)
        {
            var keyboardProvider =
                features.FirstOrDefault(x => Regex.IsMatch(cabinetType.ToString(), x.CabinetTypeRegex)) ??
                new FeaturesKeyboardProvider();

            _properties.Add(
                ApplicationConstants.KeyboardProvider,
                Tuple.Create(
                    (object)keyboardProvider.KeyboardProviderType,
                    ApplicationConstants.KeyboardProvider,
                    false));
        }

        private void SetCabinetConfiguration()
        {
            var sr = new StreamReader(CabinetFeaturesXml);
            Features cabinetFeatures;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(sr.ReadToEnd())))
            {
                var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(Features))
                    .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
                var serializer = new XmlSerializer(typeof(Features), theXmlRootAttribute ?? new XmlRootAttribute(nameof(Features)));
                cabinetFeatures = (Features)serializer.Deserialize(stream);
            }

            var cabinetType = ServiceManager.GetInstance().GetService<ICabinetDetectionService>().Type;
            Logger.Debug($"SetCabinetConfiguration - cabinetType {cabinetType}");

            if (cabinetFeatures.ScreenBrightnessControl != null)
            {
                InitializeScreenBrightnessProperties(cabinetFeatures.ScreenBrightnessControl, cabinetType);
            }
            if (cabinetFeatures.EdgeLightBrightnessControl != null)
            {
                InitializeEdgeLightBrightnessProperties(cabinetFeatures.EdgeLightBrightnessControl, cabinetType);
            }
            if (cabinetFeatures.BottomStrip != null)
            {
                InitializeBottomStripProperties(cabinetFeatures.BottomStrip, cabinetType);
            }
            if (cabinetFeatures.EdgeLightAsTowerLight != null)
            {
                InitializeEdgeLightAsTowerLightProperties(cabinetFeatures.EdgeLightAsTowerLight, cabinetType);
            }
            if (cabinetFeatures.Barkeeper != null)
            {
                InitializeBarkeeperProperties(cabinetFeatures.Barkeeper, cabinetType);
            }
            if (cabinetFeatures.SoundChannel != null)
            {
                InitializeSoundChannelProperties(cabinetFeatures.SoundChannel, cabinetType);
            }
            if (cabinetFeatures.MalfunctionMessage != null)
            {
                InitializeMalfunctionMessageProperties(cabinetFeatures.MalfunctionMessage, cabinetType);
            }
            if (cabinetFeatures.UniversalInterfaceBox != null)
            {
                InitializeUniversalInterfaceBoxProperties(cabinetFeatures.UniversalInterfaceBox, cabinetType);
            }
            if (cabinetFeatures.HarkeyReelController != null)
            {
                InitializeHarkeyReelControllerProperties(cabinetFeatures.HarkeyReelController, cabinetType);
            }
            if (cabinetFeatures.DisplayElementsControl != null)
            {
                InitializeDisplayElementsControllerProperties(cabinetFeatures.DisplayElementsControl, cabinetType);
            }
            if (cabinetFeatures.BeagleBone != null)
            {
                InitializeBeagleBoneProperties(cabinetFeatures.BeagleBone, cabinetType);
            }
            if (cabinetFeatures.DisplayLightingPage != null)
            {
                InitializeDisplayEdgeLightingPageProperties(cabinetFeatures.DisplayLightingPage, cabinetType);
			}
            if (cabinetFeatures.KeyboardProvider != null)
            {
                InitializeKeyboardProviderProperties(cabinetFeatures.KeyboardProvider, cabinetType);
            }
        }

        private void ChangeBrightness(int brightness, bool allMonitors = false)
        {
            var cabinetDetectionService = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
            var monitors = cabinetDetectionService.ExpectedDisplayDevices;
            foreach (var monitor in monitors)
            {
                monitor.Brightness = brightness;
                if (!allMonitors)
                {
                    break;
                }
            }
        }
    }
}
