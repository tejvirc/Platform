namespace Aristocrat.Monaco.UI.Common.Extensions
{
    using System;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    ///     Enum extension methods
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Return the Description attribute of the enum value if it has one. 
        /// Otherwise return the enum as a string value.
        /// </summary>
        public static string GetDescription(this object enumValue, Type type)
        {
            return type
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() is DescriptionAttribute descriptionAttribute
                ? descriptionAttribute.Description
                : enumValue.ToString();
        }
    }
}
