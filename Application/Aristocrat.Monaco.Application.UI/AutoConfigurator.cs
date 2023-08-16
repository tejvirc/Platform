namespace Aristocrat.Monaco.Application.UI
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Windows.Controls.Primitives;
    using System.Xml;
    using System.Xml.Linq;
    using Contracts.ConfigWizard;
    using Kernel;
    using log4net;

    /// <summary>
    ///     A service for applying pre-determined configuration wizard values to the controls
    ///     on those configuration wizard pages.
    /// </summary>
    public class AutoConfigurator : IAutoConfigurator, IService
    {
        private const string AutoConfigurationFilePropertyName = "AutoConfigFile";
        private const string AutoConfigurationDefaultFileName = "..\\AutoConfig.xml";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<string> _falseStrings = new List<string>
        {
            "FALSE",
            "NO",
            "OFF",
            "0"
        };

        private readonly List<string> _trueStrings = new List<string>
        {
            "TRUE",
            "YES",
            "ON",
            "1"
        };

        private Dictionary<string, string> _configuration;

        /// <inheritdoc />
        public bool AutoConfigurationExists => _configuration != null;

        /// <inheritdoc />
        public bool GetValue(string fieldName, ref string fieldValue)
        {
            if (!AutoConfigurationExists)
            {
                return false;
            }

            if (!_configuration.TryGetValue(fieldName, out fieldValue))
            {
                return false;
            }

            return !string.IsNullOrEmpty(fieldValue);
        }

        /// <inheritdoc />
        public bool GetValue(string fieldName, ref bool fieldValue)
        {
            string stringValue = null;
            if (!GetValue(fieldName, ref stringValue))
            {
                return false;
            }

            var stringValueUpper = stringValue.ToUpper(CultureInfo.InvariantCulture);

            if (_trueStrings.Contains(stringValueUpper))
            {
                fieldValue = true;
                return true;
            }

            if (_falseStrings.Contains(stringValueUpper))
            {
                fieldValue = false;
                return true;
            }

            Logger.WarnFormat("Auto config property \"{0}\" has invalid boolean value \"{1}\"", fieldName, fieldValue);
            return false;
        }

        /// <inheritdoc />
        public bool SetToggleButton(ToggleButton toggle, string fieldName)
        {
            var fieldValue = false;
            if (!GetValue(fieldName, ref fieldValue))
            {
                return false;
            }

            toggle.IsChecked = fieldValue;
            return true;
        }

        /// <inheritdoc />
        public bool SetSelector(Selector selector, string fieldName)
        {
            string stringValue = null;
            if (!GetValue(fieldName, ref stringValue))
            {
                return false;
            }

            var index = selector.Items.IndexOf(stringValue);
            if (index >= 0)
            {
                selector.SelectedIndex = index;
                return true;
            }

            Logger.WarnFormat("Auto config property \"{0}\" has invalid value \"{1}\"", fieldName, stringValue);
            return false;
        }

        /// <inheritdoc />
        public string Name { get; } = "Auto Configurator";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IAutoConfigurator) };

        /// <inheritdoc />
        public void Initialize()
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var autoConfigFile = (string)propertiesManager.GetProperty(AutoConfigurationFilePropertyName, null);
            if (autoConfigFile == null)
            {
                // If no AutoConfigFile param was specified, check for a default file in the Platform directory
                var currentDirectory = Directory.GetCurrentDirectory();
                autoConfigFile = Path.Combine(currentDirectory, AutoConfigurationDefaultFileName);

                if (!File.Exists(autoConfigFile))
                {
                    Logger.Debug("Auto configuration XML file not provided.");
                    return;
                }
            }

            try
            {
                _configuration = XDocument.Load(autoConfigFile).Descendants("data").Descendants()
                    .ToDictionary(element => element.Name.LocalName, element => element.Value);
            }
            catch (SecurityException)
            {
                Logger.Error("No read permission for auto config file");
            }
            catch (FileNotFoundException)
            {
                Logger.Error("Failed to find auto config file");
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is NotSupportedException ||
                ex is UriFormatException ||
                ex is IOException)
            {
                Logger.Error("URI for auto config file is not correct");
            }
            catch (XmlException ex)
            {
                Logger.ErrorFormat("Bad auto config XML: {0}", ex.Message);
            }
        }
    }
}