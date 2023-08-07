////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="OperatorMenuConfigurationPropertiesProvider.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2016-2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorization of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application
{
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;

    /// <summary>
    ///     This class loads operator menu configurations based on jurisdiction into OperatorMenu properties and provides
    ///     get and set access as a property provider registered with the IPropertiesManager service.
    /// </summary>
    public class OperatorMenuConfigurationPropertiesProvider : IPropertyProvider
    {
        /// <summary>
        ///     Extension point path where the operator menu configuration extension should be located.
        /// </summary>
        public static string OperatorMenuConfigurationExtensionPath = "/OperatorMenu/Configuration";

        private const string Visible = "Visible";
        private const string Hidden = "Hidden";
        private const string Collapsed = "Collapsed";
        private const string Yes = "Yes";
        private const string No = "No";
        private const string AccessRules = "AccessRules";
        private const string AccessRuleSet = "AccessRuleSet";
        private const string Access = "Access";
        private const string FieldAccess = "FieldAccess";
        private const string RuleSetName = "RuleSetName";
        private const string Id = "ID";
        private const string Rule = "Rule";
        private const string AccessRuleType = "OperatorMenuConfigurationAccessRuleSet";
        private const string RuleType = "OperatorMenuConfigurationAccessRuleSetRule";
        private const string Name = "Name";
        private const string Type = "Type";
        private const string TabType = "TabType";
        private const string Setting = "Setting";
        private const string Page = "Page";
        private const string SettingType = "SettingType";
        private const string MenuType = "OperatorMenuConfigurationMenu";
        private const string PageType = "OperatorMenuConfigurationMenuPage";
        private const string Value = "Value";
        private const string TabAccessRuleSet = "TabAccess";
        private const string PrintButtonEnabled = "PrintButtonEnabled";
        private const string PrintButtonAccess = "PrintButtonAccess";
        private const string AdditionalAccessRuleSetType = "AdditionalAccessRuleSetType";
        private const string PageName = "PageName";

        private readonly Regex _intRegex = new Regex(@"^\d$");
        private readonly Regex _doubleRegex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+.$");

        private readonly string[] _trueValues = { Visible, Yes };
        private readonly string[] _falseValues = { Hidden, Collapsed, No };

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        private void EnumerateProperties(object obj, string prefix = "")
        {
            var type = obj.GetType();
            if (obj is Array array)
            {
                var arrayType = type.Name.TrimEnd('[', ']');
                for (var i = 0; i < array.Length; i++)
                {
                    var arrayObj = array.GetValue(i);

                    if (StringCompare(arrayType, SettingType))
                    {
                        ParseSetting(arrayObj, prefix);
                    }
                    else if (StringCompare(arrayType, AdditionalAccessRuleSetType))
                    {
                        ParseAccessRule(arrayObj, prefix);
                    }
                    else if (StringCompare(arrayType, MenuType))
                    {
                        ParseMenu(arrayObj, prefix);
                    }
                    else if (StringCompare(arrayType, PageType))
                    {
                        ParsePage(arrayObj, prefix);
                    }

                    EnumerateProperties(arrayObj, prefix);
                }
            }
            else
            {
                prefix = $"{prefix}{GetPrefix(obj, type)}";
                foreach (var property in type.GetProperties())
                {
                    if (ShouldParse(property.Name, type.Name))
                    {
                        var value = type.GetProperty(property.Name)?.GetValue(obj);
                        if (value != null)
                        {
                            if (!ParseValue(prefix + property.Name, value, property.PropertyType))
                            {
                                EnumerateProperties(value, prefix);
                            }
                        }
                    }
                }
            }
        }

        private bool ShouldParse(string settingName, string typeName)
        {
            return !(StringCompare(typeName, SettingType) ||
                     StringCompare(typeName, AdditionalAccessRuleSetType) ||
                     (StringCompare(typeName, MenuType) && !StringCompare(settingName, Page)) ||
                     (StringCompare(typeName, PageType) && (!StringCompare(settingName, Setting) && !StringCompare(settingName, AccessRuleSet))) ||
                     (StringCompare(typeName, AccessRuleType) && !StringCompare(settingName, AccessRuleSet)) ||
                     (StringCompare(typeName, RuleType) && !StringCompare(settingName, Rule)) ||
                      StringCompare(settingName, AccessRules));

        }

        private string GetPrefix(object obj, Type type)
        { 
            if (StringCompare(type.Name, MenuType))
            {
                return string.Empty;
            }

            if (StringCompare(type.Name, PageType))
            {
                return$"{(string)type.GetProperty(Type)?.GetValue(obj)}.";
            }

            return $"{type.Name}.";
        }

        private bool ParseValue(string name, object value, Type propertyType)
        {
            bool parsed = false;

            if (propertyType == typeof(bool) || propertyType == typeof(int) ||
                propertyType == typeof(uint) ||
                propertyType == typeof(float) || propertyType == typeof(double))
            {
                AddProperty(name, value);
                parsed = true;
            }
            else if (propertyType == typeof(string))
            {
                var text = (value as string)?.ToUpper();
                bool? flag = null;
                if (_trueValues.Contains(text))
                {
                    flag = true;
                }
                else if (_falseValues.Contains(text))
                {
                    flag = false;
                }

                AddProperty(name, flag ?? value);
                parsed = true;
            }

            return parsed;
        }

        private void ParseSetting(object obj, string prefix)
        {
            var type = obj.GetType();

            var settingName = (string)type.GetProperty(Name)?.GetValue(obj);
            var settingValue = type.GetProperty(Value)?.GetValue(obj);
            if (settingValue == null)
            {
                return;
            }
            
            if (bool.TryParse(settingValue.ToString(), out bool flag))
            {
                settingValue = flag;
            }
            else if (_intRegex.IsMatch(settingValue.ToString()))
            {
                if (int.TryParse(settingValue.ToString(), out int value))
                {
                    settingValue = value;
                }
            }
            else if (_doubleRegex.IsMatch(settingValue.ToString()))
            {
                if (double.TryParse(settingValue.ToString(), out double value))
                {
                    settingValue = value;
                }
            }
            else
            {
                settingValue = settingValue.ToString();
            }

            AddProperty($"{prefix}{settingName}", settingValue);
        }

        private void ParseAccessRule(object obj, string prefix)
        {
            var type = obj.GetType();

            var ruleSetName = (string)type.GetProperty(RuleSetName)?.GetValue(obj);
            var id = (string)type.GetProperty(Id)?.GetValue(obj);

            AddProperty($"{prefix}{AccessRuleSet}.{id}", ruleSetName);
        }

        private void ParseMenu(object obj, string prefix)
        {
            var type = obj.GetType();

            var menuNType = (string)type.GetProperty(Type)?.GetValue(obj);
            var visible = (bool)(type.GetProperty(Visible)?.GetValue(obj) ?? false);
            var accessRule = (string)type.GetProperty(Access)?.GetValue(obj);
            prefix = $"{prefix}{menuNType}.";

            AddProperty($"{prefix}{Visible}", visible);
            AddProperty($"{prefix}{AccessRuleSet}", accessRule);
        }

        private void ParsePage(object obj, string prefix)
        {
            var type = obj.GetType();

            var pageType = (string)type.GetProperty(Type)?.GetValue(obj);
            var tabType = (string)type.GetProperty(TabType)?.GetValue(obj);
            var visible = (bool)(type.GetProperty(Visible)?.GetValue(obj) ?? false);
            var accessRule = (string)type.GetProperty(Access)?.GetValue(obj);
            var tabAccessRule = (string)type.GetProperty(TabAccessRuleSet)?.GetValue(obj);
            var fieldAccessRule = (string)type.GetProperty(FieldAccess)?.GetValue(obj);
            var printButtonEnabled = (bool)(type.GetProperty(PrintButtonEnabled)?.GetValue(obj) ?? false);
            var printButtonAccess = (string)type.GetProperty(PrintButtonAccess)?.GetValue(obj);
            var pageName = (string)type.GetProperty(PageName)?.GetValue(obj);

            if (!string.IsNullOrEmpty(pageType))
            {
                var pagePrefix = $"{prefix}{pageType}.";
                AddProperty($"{pagePrefix}{AccessRuleSet}", accessRule);
                AddProperty($"{pagePrefix}{AccessRuleSet}.{FieldAccess}", fieldAccessRule);
                AddProperty($"{pagePrefix}{PrintButtonEnabled}", printButtonEnabled);
                AddProperty($"{pagePrefix}{PrintButtonAccess}", printButtonAccess);
            }

            if (!string.IsNullOrEmpty(tabType))
            {
                var tabPrefix = $"{prefix}{tabType}.";
                AddProperty($"{tabPrefix}{Visible}", visible);
                AddProperty($"{tabPrefix}{AccessRuleSet}", tabAccessRule);
                AddProperty($"{tabPrefix}{PageName}", pageName);
            }
        }

        private void AddProperty(string name, object value)
        {
            if (!_properties.ContainsKey(name))
            {
                _properties.Add(name, value);
            }
            else
            {
                Logger.Error($"Attempt to add duplicate Operator Menu Property: {name} {value}");
            }
        }

        public OperatorMenuConfigurationPropertiesProvider()
        {
            EnumerateProperties(DeserializeConfiguration(GetOperatorMenuConfiguration()));

            var serviceManager = ServiceManager.GetInstance();
            var propertiesManager = serviceManager.GetService<IPropertiesManager>();
            propertiesManager.AddPropertyProvider(this);
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection
            =>
                new List<KeyValuePair<string, object>>(
                    _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value)));

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out object value))
            {
                return value;
            }

            var errorMessage = "Unknown operator menu property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out object value))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            _properties[propertyName] = Tuple.Create(propertyValue, value);
        }

        /// <summary>Deserialize the <c>OperatorMenu</c>.config.xml file configuration.</summary>
        /// <param name="configurationFileName">The configuration file to deserialize.</param>
        /// <returns>Object containing deserialized values from the <c>OperatorMenu</c>.config.xml file configuration</returns>
        private OperatorMenuConfiguration DeserializeConfiguration(string configurationFileName)
        {
            OperatorMenuConfiguration configuration;

            try
            {
                var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(OperatorMenuConfiguration))
                    .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
                var serializer = new XmlSerializer(typeof(OperatorMenuConfiguration), theXmlRootAttribute ?? new XmlRootAttribute(nameof(OperatorMenuConfiguration)));
                using (var reader = new StreamReader(configurationFileName))
                {
                    configuration = serializer.Deserialize(reader) as OperatorMenuConfiguration;
                }
            }
            catch (ArgumentException exception)
            {
                Logger.ErrorFormat(
                    CultureInfo.CurrentCulture,
                    "Exception occurred while deserializing OperatorMenu configuration. Exception: {0}.",
                    exception.ToString());
                throw;
            }
            catch (InvalidOperationException exception)
            {
                Logger.ErrorFormat(
                    CultureInfo.CurrentCulture,
                    "Exception occurred while deserializing OperatorMenu configuration. Exception: {0}.",
                    exception.ToString());
                throw;
            }

            return configuration;
        }

        /// <summary>Gets the path of the operator menu configuration.</summary>
        /// <returns>The path of the operator menu configuration.</returns>
        private string GetOperatorMenuConfiguration()
        {
            string path;

            try
            {
                var node =
                    MonoAddinsHelper.GetSingleSelectedExtensionNode<FilePathExtensionNode>(
                        OperatorMenuConfigurationExtensionPath);
                path = node.FilePath;
                Logger.DebugFormat(
                    CultureInfo.CurrentCulture,
                    "Found {0} node: {1}",
                    OperatorMenuConfigurationExtensionPath,
                    node.FilePath);
            }
            catch (ConfigurationErrorsException)
            {
                Logger.ErrorFormat(
                    CultureInfo.CurrentCulture,
                    "Extension path {0} not found",
                    OperatorMenuConfigurationExtensionPath);
                throw;
            }

            return path;
        }

        private bool StringCompare(string value1, string value2)
        {
            return string.Compare(value1, value2, StringComparison.InvariantCultureIgnoreCase) == 0;
        } 
    }
}
