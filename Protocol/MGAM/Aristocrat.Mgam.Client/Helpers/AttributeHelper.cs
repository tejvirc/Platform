namespace Aristocrat.Mgam.Client.Helpers
{
    using System;
    using System.Linq;
    using Attribute;
    using Protocol;

    /// <summary>
    ///     Attribute extension and helper methods.
    /// </summary>
    public static class AttributeHelper
    {
        /// <summary>
        ///     Converts string to <see cref="AttributeScope"/>.
        /// </summary>
        /// <param name="scope">Attribute scope.</param>
        /// <returns><see cref="AttributeScope"/>.</returns>
        public static AttributeScope ToAttributeScope(this string scope)
        {
            switch (scope)
            {
                case "instance":
                    return AttributeScope.Instance;
                case "application":
                    return AttributeScope.Application;
                case "installation":
                    return AttributeScope.Installation;
                case "device":
                    return AttributeScope.Device;
                case "site":
                    return AttributeScope.Site;
                case "system":
                    return AttributeScope.System;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }

        /// <summary>
        ///     Converts <see cref="T:Aristocrat.Mgam.Client.Protocol.ScopeType"/> to <see cref="AttributeScope"/>.
        /// </summary>
        /// <param name="scope">Attribute scope.</param>
        /// <returns><see cref="AttributeScope"/>.</returns>
        public static AttributeScope ToAttributeScope(this ScopeType scope)
        {
            switch (scope)
            {
                case ScopeType.instance:
                    return AttributeScope.Instance;
                case ScopeType.application:
                    return AttributeScope.Application;
                case ScopeType.installation:
                    return AttributeScope.Installation;
                case ScopeType.device:
                    return AttributeScope.Device;
                case ScopeType.site:
                    return AttributeScope.Site;
                case ScopeType.system:
                    return AttributeScope.System;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }

        /// <summary>
        ///     Converts attribute from string to value data type.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">Attribute value as string.</param>
        /// <returns>Attribute value.</returns>
        public static object ConvertValue(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var attribute = SupportedAttributes.Get().Single(x => x.Name == name);

            switch (attribute.Type)
            {
                case AttributeValueType.Integer:
                    return ConvertToInt();
                case AttributeValueType.Decimal:
                    return ConvertToDouble();
                case AttributeValueType.String:
                    return value ?? string.Empty;
                case AttributeValueType.Boolean:
                    return ConvertToBool();
                default:
                    throw new ArgumentOutOfRangeException(nameof(name), $@"The attribute type is invalid for conversion: {attribute.Type}");
            }

            int ConvertToInt()
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return default;
                }

                if (!int.TryParse(value, out var newValue))
                {
                    throw new InvalidOperationException($"Invalid value ({value}) for {name} attribute");
                }

                return newValue;
            }

            bool ConvertToBool()
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return default;
                }

                if (!bool.TryParse(value, out var newValue))
                {
                    throw new InvalidOperationException($"Invalid value ({value}) for {name} attribute");
                }

                return newValue;
            }

            double ConvertToDouble()
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return default;
                }

                if (!double.TryParse(value, out var newValue))
                {
                    throw new InvalidOperationException($"Invalid value ({value}) for {name} attribute");
                }

                return newValue;
            }
        }
    }
}
