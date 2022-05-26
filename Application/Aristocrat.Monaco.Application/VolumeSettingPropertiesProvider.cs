namespace Aristocrat.Monaco.Application
{
    using Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System;
    using Aristocrat.Monaco.Hardware.Contracts;

    /// <summary>
    ///     A class that provides the properties for selected addin configurations.
    /// </summary>
    public class VolumeSettingPropertiesProvider : IPropertyProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<string, Tuple<object, bool>> _properties=new Dictionary<string, Tuple<object, bool>>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfiguredAddinsPropertiesProvider" /> class.
        /// </summary>
        public VolumeSettingPropertiesProvider()
        {
            var configVolumeSettings = ConfigurationUtilities.GetConfiguration<VolumeSetting>("/Audio/VolumeSettingConfiguration", () => null);

            if (configVolumeSettings == null)
                return;

            var _volumePresets = LoadVolumeLevel(configVolumeSettings.MasterVolumeSettings);
            var _volumeScalarPresets=LoadVolumeScalar(configVolumeSettings.VolumeScalarSettings);

            _properties = new Dictionary<string, Tuple<object, bool>>
            {
                { HardwareConstants.VolumePreset, Tuple.Create((object)_volumePresets,false) },
                { HardwareConstants.VolumeScalarPreset, Tuple.Create((object)_volumeScalarPresets,false) },
            };
        }


        private Dictionary<VolumeLevel, float> LoadVolumeLevel(VolumeSettingVolumeNode[] masterVolumeSettings)
        {
            var result = new Dictionary<VolumeLevel, float>();
            foreach (var i in masterVolumeSettings)
            {
                VolumeLevel level;

                switch (i.Key)
                {
                    case VolumeLevelSetting.ExtraLow:
                        level = VolumeLevel.ExtraLow;
                        break;
                    case VolumeLevelSetting.Low:
                        level = VolumeLevel.Low;
                        break;
                    case VolumeLevelSetting.MediumLow:
                        level = VolumeLevel.MediumLow;
                        break;
                    case VolumeLevelSetting.Medium:
                        level = VolumeLevel.Medium;
                        break;
                    case VolumeLevelSetting.MediumHigh:
                        level = VolumeLevel.MediumHigh;
                        break;
                    case VolumeLevelSetting.High:
                        level = VolumeLevel.High;
                        break;
                    case VolumeLevelSetting.ExtraHigh:
                        level = VolumeLevel.ExtraHigh;
                        break;
                    default:
                        throw new Exception("Unkown volume level!");
                }

                result.Add(level, i.Value);
            }
            return result;
        }

        private Dictionary<VolumeScalar, float> LoadVolumeScalar(VolumeSettingScalarNode[] VolumeScalarSettings)
        {
            var result = new Dictionary<VolumeScalar, float>();
            foreach (var i in VolumeScalarSettings)
            {
                VolumeScalar scalar;

                switch (i.Key)
                {
                    case VolumeScalarSetting.Scalar20:
                        scalar = VolumeScalar.Scale20;
                        break;
                    case VolumeScalarSetting.Scalar40:
                        scalar = VolumeScalar.Scale40;
                        break;
                    case VolumeScalarSetting.Scalar60:
                        scalar = VolumeScalar.Scale60;
                        break;
                    case VolumeScalarSetting.Scalar80:
                        scalar = VolumeScalar.Scale80;
                        break;
                    case VolumeScalarSetting.Scalar100:
                        scalar = VolumeScalar.Scale100;
                        break;
                    default:
                        throw new Exception("Unkown volume scalar!");
                }

                result.Add(scalar, i.Value);
            }
            return result;
        }

        /// <summary>
        ///     Gets a reference to a property provider collection of properties.
        /// </summary>
        /// <returns>A read only reference to a collection.</returns>
        public ICollection<KeyValuePair<string, object>> GetCollection
            => new List<KeyValuePair<string, object>>(
                _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Item1)));

        /// <summary>
        ///     Gets the selected configuration property
        /// </summary>
        /// <param name="propertyName"> Should be the ConfigurationSelectedKey property. </param>
        /// <returns> A selected configuration property. </returns>
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

        /// <summary>
        ///     Sets the list of selected configurations.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"> </param>
        /// <remarks>
        ///     This method should not be called as these properties are readonly from xml file.
        /// </remarks>
        public void SetProperty(string propertyName, object propertyValue)
        {
            throw new NotImplementedException();
        }

    }
}