namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts.OperatorMenu;
    using Kernel;
    using log4net;

    public class OperatorMenuConfigurationService : IOperatorMenuConfiguration, IService
    {
        public enum OperatorMenuConfigType
        {
            Menu,
            PageTab,
            Page
        }

        private const string ConfigRoot = "OperatorMenuConfiguration";
        private const string AccessRuleSet = "AccessRuleSet";
        private const string PrintAccessRuleSet = "PrintButtonAccess";
        private const string PrintButtonEnabled = "PrintButtonEnabled";
        private const string Visible = "Visible";
        private const string PageName = "PageName";
        private const string ViewAllLogs = "ViewAllLogs";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _properties;

        public OperatorMenuConfigurationService()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuConfigurationService" /> class.
        /// </summary>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        public OperatorMenuConfigurationService(
            IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public string GetAccessRuleSet(IOperatorMenuConfigObject obj)
        {
            return GetProperty(obj.GetType(), AccessRuleSet, string.Empty);
        }

        public string GetPrintAccessRuleSet(IOperatorMenuConfigObject obj)
        {
            return GetProperty(obj.GetType(), PrintAccessRuleSet, string.Empty);
        }

        public bool GetVisible(IOperatorMenuConfigObject obj)
        {
            return GetProperty(obj.GetType(), Visible, true);
        }

        public bool GetVisible(Type type)
        {
            return GetProperty(type, Visible, true);
        }

        public bool GetVisible(string objectName)
        {
            return GetProperty(objectName, Visible, true);
        }

        public string GetPageName(IOperatorMenuConfigObject obj)
        {
            return GetProperty(obj.GetType(), PageName, string.Empty);
        }

        public bool GetPrintButtonEnabled(IOperatorMenuConfigObject obj, bool defaultValue)
        {
            return GetProperty(obj.GetType(), PrintButtonEnabled, defaultValue);
        }

        public string GetAccessRuleSet(IOperatorMenuConfigObject page, string id)
        {
            return GetProperty(page.GetType(), $"{AccessRuleSet}.{id}", string.Empty);
        }

        public T GetSetting<T>(IOperatorMenuPageViewModel page, string settingName, T defaultValue)
        {
            var configName = page?.GetType().Name ?? string.Empty;
            var propertyName = string.IsNullOrEmpty(configName)
                ? $"{ConfigRoot}.{settingName}"
                : $"{ConfigRoot}.{configName}.{settingName}";

            return (T)_properties.GetProperty(propertyName, defaultValue);
        }

        public T GetSetting<T>(string settingName, T defaultValue)
        {
            return GetSetting((IOperatorMenuPageViewModel)null, settingName, defaultValue);
        }

        public T GetSetting<T>(Type type, string settingName, T defaultValue)
        {
            var propertyName = type == null ? $"{ConfigRoot}.{settingName}" : $"{ConfigRoot}.{type.Name}.{settingName}";

            return (T)_properties.GetProperty(propertyName, defaultValue);
        }

        public T GetSetting<T>(string typeName, string settingName, T defaultValue)
        {
            var propertyName = $"{ConfigRoot}.{typeName}.{settingName}";

            return (T)_properties.GetProperty(propertyName, defaultValue);
        }

        public void Initialize()
        {
            Logger.Debug(Name + " OnInitialize()");
        }

        /// <inheritdoc />
        public string Name => "Operator Menu Configuration Service";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IOperatorMenuConfiguration) };

        private T GetProperty<T>(Type type, string property, T defaultValue)
        {
            return GetProperty(type.Name, property, defaultValue);
        }

        private T GetProperty<T>(string typeName, string property, T defaultValue)
        {
            var propertyName = $"{ConfigRoot}.{typeName}.{property}";
            return (T)_properties.GetProperty(propertyName, defaultValue);
        }
    }
}