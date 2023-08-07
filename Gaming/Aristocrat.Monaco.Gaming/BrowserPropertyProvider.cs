namespace Aristocrat.Monaco.Gaming
{
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;

    public class BrowserPropertyProvider : IPropertyProvider
    {
        private const string ConfigurationExtensionPath = "/Browser/Configuration";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly Dictionary<string, object> _properties;

        public BrowserPropertyProvider()
        {
            var config = ConfigurationUtilities.GetConfiguration(
                ConfigurationExtensionPath,
                () => new BrowserConfiguration { Browser = new BrowserConfigurationBrowser() });

            _properties = new Dictionary<string, object>
            {
                { GamingConstants.BrowserMaxCpuPerProcess, config.Browser.MaxCpuPerProcess },
                { GamingConstants.BrowserMaxCpuTotal, config.Browser.MaxCpuTotal },
                { GamingConstants.BrowserMaxMemoryPerProcess, config.Browser.MaxMemoryPerProcess },
                { GamingConstants.MonitorBrowserProcess, config.Browser.MonitorBrowserProcess }
            };
        }

        public ICollection<KeyValuePair<string, object>> GetCollection =>
            new List<KeyValuePair<string, object>>(_properties);

        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value;
            }

            var errorMessage = "Unknown browser property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        public void SetProperty(string propertyName, object propertyValue)
        {
            if (string.IsNullOrEmpty(propertyName) || !_properties.TryGetValue(propertyName, out var value))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            if (value == propertyValue)
            {
                return;
            }

            Logger.Debug($"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");
            _properties[propertyName] = propertyValue;
        }
    }
}
