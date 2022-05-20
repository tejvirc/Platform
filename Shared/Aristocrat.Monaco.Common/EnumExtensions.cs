namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    ///     <see cref="Enum" /> extension methods.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Gets list of flag values.
        /// </summary>
        /// <param name="this">Enum value</param>
        /// <returns>List of <see cref="Enum" /> values.</returns>
        public static IEnumerable<T> GetFlags<T>(this T @this)
            where T : Enum
        {
            return Enum.GetValues(@this.GetType()).Cast<T>().Where(x => @this.HasFlag(x));
        }

        /// <summary>
        ///     Returns adorned attribute value of enum
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="this">The enum value</param>
        /// <returns>The associated attribute if it exists</returns>
        public static TAttribute GetAttribute<TAttribute>(this Enum @this)
            where TAttribute : Attribute
        {
            var enumType = @this.GetType();
            var name = Enum.GetName(enumType, @this);

            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }

        /// <summary>
        ///     Parses the provided string into the provided enumeration type
        /// </summary>
        /// <param name="this">The string to convert into an enumeration</param>
        /// <typeparam name="T">The type of enumeration you want to convert into</typeparam>
        /// <returns>The converted string into the provided enumeration type</returns>
        public static T ToEnumeration<T>(this string @this)
            where T : Enum
        {
            return (T)Enum.Parse(typeof(T), @this);
        }

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

        /// <summary>
        ///     Return the Description attribute of the enum value if it has one.
        /// Otherwise return the enum as a string value.
        /// </summary>
        public static string GetDescription(this Enum enumValue)
        {
            var enumType = enumValue.GetType();

            var attribute = enumType
                .GetField(Enum.GetName(enumType, enumValue))
                .GetCustomAttributes(false)
                .OfType<DescriptionAttribute>()
                .FirstOrDefault();

            return attribute?.Description ?? enumValue.ToString();
        }
    }
}